// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using OtpNet;
using OroIdentityServer.Infraestructure;
using OroIdentityServer.Server.Tests.Infrastructure;
using Xunit;

namespace OroIdentityServer.Server.Tests.Endpoints;

/// <summary>
/// End-to-end coverage of the second factor: enrol from the console, sign in, get challenged,
/// clear the challenge. The flow under test is the security boundary itself - no admin cookie and
/// no <c>UserSession</c> may exist until the code is accepted - so every assertion looks at the
/// cookie and at the database, not only at the redirect.
/// </summary>
[Collection(nameof(AspireTestCollection))]
public sealed class MfaLoginFlowTests(AspireIdentityServerApp app)
{
    private const string AdminUser = "admin";
    private const string AdminPassword = "Admin@123456";

    private static readonly JsonSerializerOptions WebJson = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task Login_WithoutMfa_BehavesAsBefore()
    {
        using var client = app.CreateClient();

        var response = await LoginAsync(client);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/", response.Headers.Location?.OriginalString);
        Assert.Contains("oro.identity.admin", SetCookies(response));
    }

    [Fact]
    public async Task Login_WithMfaEnabled_ChallengesBeforeIssuingAnySession()
    {
        using var enrolClient = app.CreateClient();
        await EnrollAsync(enrolClient);

        // A separate client: the enrolment one still holds the admin cookie it was issued
        // before the factor existed, which would mask "no session was issued here".
        using var client = app.CreateClient();

        try
        {
            var before = await ActiveSessionTokensAsync();

            var response = await LoginAsync(client);

            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Equal("/Account/Mfa", response.Headers.Location?.OriginalString);

            var cookies = SetCookies(response);
            Assert.Contains("oro.identity.mfa.pending", cookies);
            // The password is not enough: no admin cookie and no session row yet.
            Assert.DoesNotContain("oro.identity.admin", cookies);
            Assert.Empty(await NewSessionsSinceAsync(before));

            // And the API stays closed while the second factor is pending.
            var api = await client.GetAsync("/api/users");
            Assert.Equal(HttpStatusCode.Unauthorized, api.StatusCode);
        }
        finally
        {
            await ResetMfaAsync();
        }
    }

    [Fact]
    public async Task Mfa_WithWrongCode_KeepsTheUserOut()
    {
        using var enrolClient = app.CreateClient();
        await EnrollAsync(enrolClient);

        // A separate client: the enrolment one still holds the admin cookie it was issued
        // before the factor existed, which would mask "no session was issued here".
        using var client = app.CreateClient();

        try
        {
            var before = await ActiveSessionTokensAsync();
            var login = await LoginAsync(client);
            Assert.Equal("/Account/Mfa", login.Headers.Location?.OriginalString);

            var wrong = await SubmitCodeAsync(client, "000001");

            Assert.Equal(HttpStatusCode.Redirect, wrong.StatusCode);
            Assert.StartsWith("/Account/Mfa?error=invalid", wrong.Headers.Location?.OriginalString);
            Assert.DoesNotContain("oro.identity.admin", SetCookies(wrong));
            Assert.Empty(await NewSessionsSinceAsync(before));

            var api = await client.GetAsync("/api/users");
            Assert.Equal(HttpStatusCode.Unauthorized, api.StatusCode);
        }
        finally
        {
            await ResetMfaAsync();
        }
    }

    [Fact]
    public async Task Mfa_WithValidCode_IssuesTheSessionAndClearsThePendingCookie()
    {
        using var client = app.CreateClient();
        var factor = await EnrollAsync(client);

        try
        {
            var before = await ActiveSessionTokensAsync();
            await LoginAsync(client);

            var accepted = await SubmitCodeAsync(client, await NextWindowCodeAsync(factor.ManualKey));

            Assert.Equal(HttpStatusCode.Redirect, accepted.StatusCode);
            Assert.Equal("/", accepted.Headers.Location?.OriginalString);

            var cookies = SetCookies(accepted);
            Assert.Contains("oro.identity.admin", cookies);
            // The pending cookie is cleared: oro.identity.mfa.pending=; expires=Thu, 01 Jan 1970
            Assert.Matches("oro.identity.mfa.pending=;", cookies);

            Assert.Single(await NewSessionsSinceAsync(before));

            var api = await client.GetAsync("/api/users");
            Assert.Equal(HttpStatusCode.OK, api.StatusCode);
        }
        finally
        {
            await ResetMfaAsync();
        }
    }

    [Fact]
    public async Task Mfa_LocksTheChallengeAfterFiveWrongCodes()
    {
        using var client = app.CreateClient();
        var factor = await EnrollAsync(client);

        try
        {
            var before = await ActiveSessionTokensAsync();
            await LoginAsync(client);

            for (var attempt = 1; attempt < MfaLoginChallengeMaxAttempts; attempt++)
            {
                var response = await SubmitCodeAsync(client, "000001");
                Assert.StartsWith("/Account/Mfa?error=invalid", response.Headers.Location?.OriginalString);
            }

            // The fifth wrong code spends both budgets at once. The per-credential lockout wins
            // because it is the stronger one: it does not clear by retyping the password.
            var locked = await SubmitCodeAsync(client, "000001");
            Assert.StartsWith("/Account/Mfa?error=lockedout", locked.Headers.Location?.OriginalString);
            Assert.Contains("retryAfter=", locked.Headers.Location?.OriginalString);

            // The correct code no longer helps: the challenge is spent, and the credential is
            // serving a lockout.
            var afterLock = await SubmitCodeAsync(client, await NextWindowCodeAsync(factor.ManualKey));
            Assert.StartsWith("/Account/Mfa?error=locked", afterLock.Headers.Location?.OriginalString);
            Assert.DoesNotContain("oro.identity.admin", SetCookies(afterLock));
            Assert.Empty(await NewSessionsSinceAsync(before));
        }
        finally
        {
            await ResetMfaAsync();
        }
    }

    [Fact]
    public async Task Mfa_Lockout_SurvivesRepeatedFreshPasswordAttempts()
    {
        using var enrolClient = app.CreateClient();
        var factor = await EnrollAsync(enrolClient);

        try
        {
            // Each round is a brand new client: a new password step, a new challenge, a fresh
            // per-challenge counter. That is exactly the loop a brute forcer runs, and the reason a
            // per-challenge budget on its own is not a real defence.
            for (var round = 1; round <= MfaLoginChallengeMaxAttempts; round++)
            {
                using var attacker = app.CreateClient();
                Assert.Equal("/Account/Mfa", (await LoginAsync(attacker)).Headers.Location?.OriginalString);
                await SubmitCodeAsync(attacker, "000001");
            }

            // One more, completely fresh login: the correct code is now refused because the
            // credential itself is serving a lockout, not because a challenge is spent.
            using var blocked = app.CreateClient();
            Assert.Equal("/Account/Mfa", (await LoginAsync(blocked)).Headers.Location?.OriginalString);

            var before = await ActiveSessionTokensAsync();
            var response = await SubmitCodeAsync(blocked, await NextWindowCodeAsync(factor.ManualKey));

            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.StartsWith("/Account/Mfa?error=lockedout", response.Headers.Location?.OriginalString);
            Assert.Contains("retryAfter=", response.Headers.Location?.OriginalString);
            Assert.DoesNotContain("oro.identity.admin", SetCookies(response));
            Assert.Empty(await NewSessionsSinceAsync(before));
        }
        finally
        {
            await ResetMfaAsync();
        }
    }

    [Fact]
    public async Task Mfa_AcceptsARecoveryCode()
    {
        using var client = app.CreateClient();
        var factor = await EnrollAsync(client);

        try
        {
            var before = await ActiveSessionTokensAsync();
            await LoginAsync(client);

            var accepted = await SubmitCodeAsync(client, factor.RecoveryCode);

            Assert.Equal(HttpStatusCode.Redirect, accepted.StatusCode);
            Assert.Equal("/", accepted.Headers.Location?.OriginalString);
            Assert.Contains("oro.identity.admin", SetCookies(accepted));
            Assert.Single(await NewSessionsSinceAsync(before));
        }
        finally
        {
            await ResetMfaAsync();
        }
    }

    [Fact]
    public async Task Mfa_Challenge_SurvivesAReloadOfTheForm()
    {
        using var client = app.CreateClient();
        var factor = await EnrollAsync(client);

        try
        {
            await LoginAsync(client);

            // The pending cookie is the whole state: rendering the page must not consume it.
            var page = await client.GetAsync("/Account/Mfa");
            Assert.Equal(HttpStatusCode.OK, page.StatusCode);

            var accepted = await SubmitCodeAsync(client, await NextWindowCodeAsync(factor.ManualKey));
            Assert.Equal("/", accepted.Headers.Location?.OriginalString);
        }
        finally
        {
            await ResetMfaAsync();
        }
    }

    [Fact]
    public async Task Mfa_QrEndpoint_RequiresThePendingCookie()
    {
        using var anonymous = app.CreateClient();
        Assert.Equal(HttpStatusCode.NotFound, (await anonymous.GetAsync("/auth/mfa/qr")).StatusCode);

        using var enrolClient = app.CreateClient();
        await EnrollAsync(enrolClient);

        // A separate client: the enrolment one still holds the admin cookie it was issued
        // before the factor existed, which would mask "no session was issued here".
        using var client = app.CreateClient();

        try
        {
            await LoginAsync(client);

            var qr = await client.GetAsync("/auth/mfa/qr");
            Assert.Equal(HttpStatusCode.OK, qr.StatusCode);
            Assert.Equal("image/png", qr.Content.Headers.ContentType?.MediaType);
        }
        finally
        {
            await ResetMfaAsync();
        }
    }

    [Fact]
    public async Task TenantPolicy_ForcesEnrolmentDuringLogin()
    {
        using var client = app.CreateClient();
        await SetTenantMfaPolicyAsync(true);

        try
        {
            var before = await ActiveSessionTokensAsync();

            var response = await LoginAsync(client);

            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Equal("/Account/MfaSetup", response.Headers.Location?.OriginalString);
            Assert.Contains("oro.identity.mfa.pending", SetCookies(response));
            Assert.DoesNotContain("oro.identity.admin", SetCookies(response));
            Assert.Empty(await NewSessionsSinceAsync(before));

            // The setup page and its QR are reachable while the challenge is pending.
            var page = await client.GetAsync("/Account/MfaSetup");
            Assert.Equal(HttpStatusCode.OK, page.StatusCode);

            var qr = await client.GetAsync("/auth/mfa/qr");
            Assert.Equal(HttpStatusCode.OK, qr.StatusCode);

            // The page renders the manual key server-side (the secret never travels in the query
            // string), which is what a user types in when they cannot scan the QR.
            var manualKey = ReadManualKey(await page.Content.ReadAsStringAsync());

            var accepted = await client.PostAsync(
                "/auth/mfa/enroll",
                new FormUrlEncodedContent(new Dictionary<string, string> { ["Code"] = CurrentCode(manualKey) }));

            Assert.Equal(HttpStatusCode.Redirect, accepted.StatusCode);
            Assert.Equal("/", accepted.Headers.Location?.OriginalString);
            Assert.Contains("oro.identity.admin", SetCookies(accepted));
            Assert.Single(await NewSessionsSinceAsync(before));

            // Now that a factor is enrolled, the next login challenges for it as usual.
            using var second = app.CreateClient();
            var secondLogin = await LoginAsync(second);
            Assert.Equal("/Account/Mfa", secondLogin.Headers.Location?.OriginalString);
        }
        finally
        {
            await SetTenantMfaPolicyAsync(false);
            await ResetMfaAsync();
        }
    }

    [Fact]
    public async Task TenantPolicy_WithAWrongEnrolmentCode_StaysOut()
    {
        using var client = app.CreateClient();
        await SetTenantMfaPolicyAsync(true);

        try
        {
            var before = await ActiveSessionTokensAsync();

            var login = await LoginAsync(client);
            Assert.Equal("/Account/MfaSetup", login.Headers.Location?.OriginalString);

            var page = await client.GetAsync("/Account/MfaSetup");
            var manualKey = ReadManualKey(await page.Content.ReadAsStringAsync());

            var rejected = await client.PostAsync(
                "/auth/mfa/enroll",
                new FormUrlEncodedContent(new Dictionary<string, string> { ["Code"] = "000001" }));

            Assert.Equal(HttpStatusCode.Redirect, rejected.StatusCode);
            Assert.StartsWith("/Account/MfaSetup?error=invalid", rejected.Headers.Location?.OriginalString);
            Assert.DoesNotContain("oro.identity.admin", SetCookies(rejected));
            Assert.Empty(await NewSessionsSinceAsync(before));

            // The factor stays unconfirmed, so the tenant policy keeps forcing enrolment.
            var status = await client.GetAsync("/api/me/mfa");
            Assert.Equal(HttpStatusCode.Unauthorized, status.StatusCode);
        }
        finally
        {
            await SetTenantMfaPolicyAsync(false);
            await ResetMfaAsync();
        }
    }

    [Fact]
    public async Task SelfServiceMfa_RequiresAnAuthenticatedSession()
    {
        using var anonymous = app.CreateClient();

        Assert.Equal(HttpStatusCode.Unauthorized, (await anonymous.GetAsync("/api/me/mfa")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await anonymous.PostAsync("/api/me/mfa/setup", null)).StatusCode);
    }

    [Fact]
    public async Task SelfServiceMfa_ReportsTheEnrolledState()
    {
        using var enrolClient = app.CreateClient();
        await EnrollAsync(enrolClient);

        // A separate client: the enrolment one still holds the admin cookie it was issued
        // before the factor existed, which would mask "no session was issued here".
        using var client = app.CreateClient();

        try
        {
            var response = await enrolClient.GetAsync("/api/me/mfa");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(body);
            var data = document.RootElement.GetProperty("data");

            Assert.True(data.GetProperty("isEnabled").GetBoolean());
            Assert.Equal(10, data.GetProperty("unusedRecoveryCodes").GetInt32());
            Assert.False(data.GetProperty("tenantRequiresMfa").GetBoolean());
        }
        finally
        {
            await ResetMfaAsync();
        }
    }

    [Fact]
    public async Task SessionExpiry_RedirectsWithTheReasonAndTheOriginalUrl()
    {
        using var client = app.CreateClient();
        var response = await LoginAsync(client);
        Assert.Contains("oro.identity.admin", SetCookies(response));

        // A dead server-side session with a still-valid cookie is what the expiry middleware sees.
        await ExpireAllSessionsAsync();

        var page = await client.GetAsync("/users");
        Assert.Equal(HttpStatusCode.Redirect, page.StatusCode);

        var location = page.Headers.Location?.OriginalString ?? string.Empty;
        Assert.StartsWith("/Account/Login?reason=expired", location);
        Assert.Contains("ReturnUrl=", location);
    }

    [Fact]
    public async Task ApiWithoutASession_Answers401WithALoginLocation()
    {
        using var client = app.CreateClient();

        var response = await client.GetAsync("/api/users");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal("/Account/Login?reason=expired", response.Headers.Location?.ToString());
    }

    // ----- helpers -------------------------------------------------------------------------

    private const int MfaLoginChallengeMaxAttempts = 5;

    private static Task<HttpResponseMessage> LoginAsync(HttpClient client)
        => client.PostAsync(
            "/auth/login",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["loginIdentifier"] = AdminUser,
                ["password"] = AdminPassword
            }));

    private static Task<HttpResponseMessage> SubmitCodeAsync(HttpClient client, string code)
        => client.PostAsync(
            "/auth/mfa",
            new FormUrlEncodedContent(new Dictionary<string, string> { ["Code"] = code }));

    /// <summary>
    /// Signs in and enrols an authenticator through the same endpoints the console uses.
    /// Returns the manual setup key plus one of the recovery codes the confirmation hands out.
    /// </summary>
    private async Task<EnrolledFactor> EnrollAsync(HttpClient client)
    {
        var login = await LoginAsync(client);
        Assert.Equal(HttpStatusCode.Redirect, login.StatusCode);

        var setup = await client.PostAsync("/api/me/mfa/setup", null);
        Assert.Equal(HttpStatusCode.OK, setup.StatusCode);

        var payload = await setup.Content.ReadFromJsonAsync<EnrollmentPayload>(WebJson);
        Assert.NotNull(payload);
        Assert.StartsWith("data:image/png;base64,", payload!.QrCodeDataUri);
        Assert.StartsWith("otpauth://totp/", payload.ProvisioningUri);

        var confirm = await client.PostAsJsonAsync("/api/me/mfa/confirm", new { code = CurrentCode(payload.ManualKey) }, WebJson);
        Assert.Equal(HttpStatusCode.OK, confirm.StatusCode);

        var confirmed = await confirm.Content.ReadFromJsonAsync<RecoveryCodesPayload>(WebJson);
        Assert.NotNull(confirmed);
        Assert.Equal(10, confirmed!.RecoveryCodes.Count);

        return new EnrolledFactor(payload.ManualKey, confirmed.RecoveryCodes[0]);
    }

    /// <summary>Reads the setup key the enrolment page renders as data-manual-key.</summary>
    private static string ReadManualKey(string html)
    {
        var match = Regex.Match(html, "data-manual-key=\"(?<key>[A-Z2-7]+)\"");
        Assert.True(match.Success, "The enrolment page did not render the manual setup key.");
        return match.Groups["key"].Value;
    }

    private static string CurrentCode(string base32Secret)
        => new Totp(Base32Encoding.ToBytes(base32Secret)).ComputeTotp(DateTime.UtcNow);

    /// <summary>
    /// Waits for the window after the current one and returns its code. Confirming an enrolment
    /// burns the time step of the code used, and the replay check then (correctly) refuses the
    /// same window, so a login that follows immediately has to wait for the next code.
    /// </summary>
    private static async Task<string> NextWindowCodeAsync(string base32Secret)
    {
        var totp = new Totp(Base32Encoding.ToBytes(base32Secret));
        var currentStep = (long)(DateTime.UtcNow - DateTime.UnixEpoch).TotalSeconds / 30;

        // Half a second of slack so the boundary cannot be crossed while the code is computed.
        var nextWindow = DateTime.UnixEpoch.AddSeconds((currentStep + 1) * 30).AddMilliseconds(500);
        var wait = nextWindow - DateTime.UtcNow;
        if (wait > TimeSpan.Zero) await Task.Delay(wait);

        return totp.ComputeTotp(DateTime.UtcNow);
    }

    private static string SetCookies(HttpResponseMessage response)
        => response.Headers.TryGetValues("Set-Cookie", out var values)
            ? string.Join("; ", values)
            : string.Empty;

    /// <summary>
    /// Session tokens currently open. Sessions live for hours and the whole collection shares one
    /// database, so a test can only assert on the rows its own requests created.
    /// </summary>
    private async Task<HashSet<string>> ActiveSessionTokensAsync()
    {
        using var scope = app.CreateDbContext();
        var tokens = await scope.UserSessions
            .AsNoTracking()
            .Where(s => s.ExpiresAt > DateTime.UtcNow)
            .Select(s => s.SessionToken!)
            .ToListAsync();

        return tokens.ToHashSet(StringComparer.Ordinal);
    }

    private async Task<List<string>> NewSessionsSinceAsync(HashSet<string> before)
    {
        var after = await ActiveSessionTokensAsync();
        after.ExceptWith(before);
        return after.ToList();
    }

    private static async Task<Guid> AdminUserIdAsync(OroIdentityAppContext context)
    {
        var user = await context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserName == AdminUser);
        Assert.NotNull(user);
        return user!.Id.Value;
    }

    /// <summary>
    /// Removes the factor from the seeded admin so the rest of the suite (which signs in without a
    /// second factor) keeps working: the tests in this collection share one database.
    /// </summary>
    private async Task ResetMfaAsync()
    {
        using var scope = app.CreateDbContext();
        var userId = await AdminUserIdAsync(scope);

        await scope.Database.ExecuteSqlInterpolatedAsync($"""DELETE FROM "MfaRecoveryCodes" WHERE "UserId" = {userId}""");
        await scope.Database.ExecuteSqlInterpolatedAsync($"""DELETE FROM "MfaLoginChallenges" WHERE "UserId" = {userId}""");
        await scope.Database.ExecuteSqlInterpolatedAsync($"""DELETE FROM "MfaCredentials" WHERE "UserId" = {userId}""");
    }

    private async Task ExpireAllSessionsAsync()
    {
        using var scope = app.CreateDbContext();
        await scope.UserSessions
            .AsNoTracking()
            .Where(s => s.ExpiresAt > DateTime.UtcNow)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.ExpiresAt, DateTime.UtcNow.AddMinutes(-1)));
    }

    /// <summary>
    /// Flips the policy on the tenant the seeded admin belongs to. Targeting the tenant by id
    /// matters: the suite creates other tenants, and "the first one with the opposite flag" is
    /// not necessarily the one whose policy governs this login.
    /// </summary>
    private async Task SetTenantMfaPolicyAsync(bool requireMfa)
    {
        using var scope = app.CreateDbContext();
        var admin = await scope.Users.AsNoTracking().FirstAsync(u => u.UserName == AdminUser);
        Assert.NotNull(admin.TenantId);

        var tenant = await scope.Tenants.FirstAsync(t => t.Id == admin.TenantId);
        tenant.UpdateMfaPolicy(requireMfa);
        await scope.SaveChangesAsync();
    }

    /// <summary>The setup key and one recovery code of a freshly enrolled factor.</summary>
    private sealed record EnrolledFactor(string ManualKey, string RecoveryCode);

    private sealed record EnrollmentPayload(string ProvisioningUri, string ManualKey, string QrCodeDataUri);

    private sealed record RecoveryCodesPayload(IReadOnlyList<string> RecoveryCodes);
}