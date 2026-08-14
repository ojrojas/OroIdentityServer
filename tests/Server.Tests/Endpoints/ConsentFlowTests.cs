// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using static OpenIddict.Abstractions.OpenIddictConstants;
using OroIdentityServer.Server.Tests.Infrastructure;
using Xunit;

namespace OroIdentityServer.Server.Tests.Endpoints;

/// <summary>
/// Exercises the interactive approval (consent) screen that is shown when an application is
/// configured with explicit consent and no prior permanent authorization covers the request.
/// </summary>
[Collection(nameof(AspireTestCollection))]
public sealed class ConsentFlowTests(AspireIdentityServerApp app)
{
    private const string AdminUser = "admin";
    private const string AdminPassword = "Admin@123456";
    private const string RedirectUri = "https://localhost/consent-test/callback";
    private const string PostLogoutRedirectUri = "https://localhost/consent-test";
    private const string Scope = "openid profile";

    [Fact]
    public async Task Authorize_ExplicitConsent_WithoutPriorAuthorization_RedirectsToConsentPage()
    {
        var client = await CreateLoggedInClientAsync();
        var clientId = await CreateExplicitConsentAppAsync(client);

        var response = await GetAuthorizeAsync(client, clientId, state: "state-abc");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        var location = response.Headers.Location!.OriginalString;
        Assert.StartsWith("/Account/Consent?", location);
        Assert.Contains("client_id=", location);
        Assert.Contains("state=state-abc", location);
    }

    [Fact]
    public async Task Authorize_ExplicitConsent_WithMatchingPermanentAuthorization_SkipsConsent()
    {
        var client = await CreateLoggedInClientAsync();
        var clientId = await CreateExplicitConsentAppAsync(client);

        await ApproveAsync(client, clientId);

        var second = await GetAuthorizeAsync(client, clientId, state: "state-second");

        Assert.Equal(HttpStatusCode.Redirect, second.StatusCode);
        var location = second.Headers.Location!.OriginalString;
        Assert.StartsWith(RedirectUri, location);
        Assert.Contains("code=", location);
        Assert.Contains("state=state-second", location);
        Assert.DoesNotContain("/Account/Consent", location);
    }

    [Fact]
    public async Task Authorize_PromptConsent_RedisplaysConsentScreen()
    {
        var client = await CreateLoggedInClientAsync();
        var clientId = await CreateExplicitConsentAppAsync(client);

        await ApproveAsync(client, clientId);

        var promptConsent = await GetAuthorizeAsync(client, clientId, state: "state-prompt", prompt: "consent");

        Assert.Equal(HttpStatusCode.Redirect, promptConsent.StatusCode);
        var location = promptConsent.Headers.Location!.OriginalString;
        Assert.StartsWith("/Account/Consent?", location);
        Assert.Contains("prompt=consent", location);
    }

    [Fact]
    public async Task Authorize_PromptNone_WithoutAuthorization_ReturnsConsentRequired()
    {
        var client = await CreateLoggedInClientAsync();
        var clientId = await CreateExplicitConsentAppAsync(client);

        var response = await GetAuthorizeAsync(client, clientId, state: "state-none", prompt: "none");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        var location = response.Headers.Location!.OriginalString;
        Assert.StartsWith(RedirectUri, location);
        Assert.Contains("error=consent_required", location);
        Assert.Contains("state=state-none", location);
    }

    [Fact]
    public async Task ConsentPage_ShowsRequestedScopesAndEchoesRequestParameters()
    {
        var client = await CreateLoggedInClientAsync();
        var clientId = await CreateExplicitConsentAppAsync(client);

        var authorize = await GetAuthorizeAsync(client, clientId, state: "state-echo");
        var (html, inputs, _) = await LoadConsentPageAsync(client, authorize.Headers.Location!.OriginalString);

        // The requested scopes are listed on the page.
        Assert.Contains("profile", html);
        Assert.Contains("openid", html);

        // Every OIDC request parameter is echoed as a hidden field.
        foreach (var key in new[]
                 {
                     "client_id", "redirect_uri", "response_type", "scope",
                     "state", "code_challenge", "code_challenge_method"
                 })
        {
            Assert.True(inputs.ContainsKey(key), $"missing hidden input '{key}'");
        }

        Assert.Equal(clientId, inputs["client_id"]);
        Assert.Equal(Scope, inputs["scope"]);
        Assert.Equal("state-echo", inputs["state"]);
    }

    [Fact]
    public async Task Consent_Approve_CompletesAuthorizationAndCreatesPermanentAuthorization()
    {
        var client = await CreateLoggedInClientAsync();
        var clientId = await CreateExplicitConsentAppAsync(client);

        var accept = await PostConsentAsync(client, clientId, "submit.Accept", "state-approve");

        Assert.Equal(HttpStatusCode.Redirect, accept.StatusCode);
        var location = accept.Headers.Location!.OriginalString;
        Assert.StartsWith(RedirectUri, location);
        Assert.Contains("code=", location);
        Assert.Contains("state=state-approve", location);

        await using (var context = app.CreateDbContext())
        {
            var count = await context.Database.SqlQueryRaw<int>(
                """
                SELECT COUNT(*) AS "Value"
                FROM "OpenIddictAuthorizations" a
                INNER JOIN "OpenIddictApplications" app ON a."ApplicationId" = app."Id"
                WHERE app."ClientId" = {0} AND a."Status" = {1} AND a."Type" = {2}
                """,
                clientId, Statuses.Valid, AuthorizationTypes.Permanent).FirstAsync();

            Assert.True(count >= 1, "no permanent authorization was created on approval");
        }
    }

    [Fact]
    public async Task Consent_Deny_ReturnsAccessDenied()
    {
        var client = await CreateLoggedInClientAsync();
        var clientId = await CreateExplicitConsentAppAsync(client);

        var deny = await PostConsentAsync(client, clientId, "submit.Deny", "state-deny");

        Assert.Equal(HttpStatusCode.Redirect, deny.StatusCode);
        var location = deny.Headers.Location!.OriginalString;
        Assert.StartsWith(RedirectUri, location);
        Assert.Contains("error=access_denied", location);
        Assert.Contains("state=state-deny", location);

        await using (var context = app.CreateDbContext())
        {
            var count = await context.Database.SqlQueryRaw<int>(
                """
                SELECT COUNT(*) AS "Value"
                FROM "OpenIddictAuthorizations" a
                INNER JOIN "OpenIddictApplications" app ON a."ApplicationId" = app."Id"
                WHERE app."ClientId" = {0} AND a."Status" = {1} AND a."Type" = {2}
                """,
                clientId, Statuses.Valid, AuthorizationTypes.Permanent).FirstAsync();

            Assert.Equal(0, count);
        }
    }

    [Fact]
    public async Task Consent_WithoutAntiforgeryToken_IsRejected()
    {
        var client = await CreateLoggedInClientAsync();
        var clientId = await CreateExplicitConsentAppAsync(client);

        var authorize = await GetAuthorizeAsync(client, clientId, state: "state-token");
        var (_, inputs, _) = await LoadConsentPageAsync(client, authorize.Headers.Location!.OriginalString);

        inputs.Remove("__RequestVerificationToken");
        inputs["submit.Accept"] = "true";

        var response = await client.PostAsync("/connect/authorize", new FormUrlEncodedContent(inputs));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private async Task<HttpClient> CreateLoggedInClientAsync()
    {
        var client = app.CreateClient();
        var login = await client.PostAsync("/auth/login", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["loginIdentifier"] = AdminUser,
            ["password"] = AdminPassword
        }));

        Assert.Equal(HttpStatusCode.Redirect, login.StatusCode);
        return client;
    }

    private static async Task<string> CreateExplicitConsentAppAsync(HttpClient client)
    {
        var clientId = $"consent-{Guid.NewGuid():N}";
        var payload = new
        {
            clientId,
            clientSecret = (string?)null,
            displayName = "Consent Test App",
            clientType = ClientTypes.Public,
            applicationType = ApplicationTypes.Web,
            consentType = ConsentTypes.Explicit,
            permissions = new[]
            {
                "ept:authorization",
                "ept:token",
                "ept:end_session",
                "ept:userinfo",
                "gt:authorization_code",
                "gt:refresh_token",
                "rst:code",
                "scp:openid",
                "scp:profile",
                "scp:email",
                "scp:roles",
                "scp:offline_access"
            },
            requirements = new[] { "ft:pkce" },
            redirectUris = new[] { RedirectUri },
            postLogoutRedirectUris = new[] { PostLogoutRedirectUri }
        };

        var response = await client.PostAsJsonAsync("/api/applications", payload);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return clientId;
    }

    /// <summary>Runs the full approve flow: authorize -> consent page -> accept.</summary>
    private static async Task ApproveAsync(HttpClient client, string clientId)
    {
        var response = await PostConsentAsync(client, clientId, "submit.Accept", $"state-{Guid.NewGuid():N}");
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("code=", response.Headers.Location!.OriginalString);
    }

    private static async Task<HttpResponseMessage> PostConsentAsync(HttpClient client, string clientId, string decision, string state)
    {
        var authorize = await GetAuthorizeAsync(client, clientId, state);
        var (_, inputs, _) = await LoadConsentPageAsync(client, authorize.Headers.Location!.OriginalString);

        inputs[decision] = "true";
        return await client.PostAsync("/connect/authorize", new FormUrlEncodedContent(inputs));
    }

    private static async Task<(string Html, Dictionary<string, string> Inputs, string Token)> LoadConsentPageAsync(HttpClient client, string consentLocation)
    {
        var response = await client.GetAsync(consentLocation);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var html = await response.Content.ReadAsStringAsync();
        var inputs = ParseHiddenInputs(html);

        Assert.True(inputs.TryGetValue("__RequestVerificationToken", out var token) && !string.IsNullOrEmpty(token),
            "the consent page must render an antiforgery token");

        return (html, inputs, token);
    }

    private static Dictionary<string, string> ParseHiddenInputs(string html)
    {
        var result = new Dictionary<string, string>();
        foreach (Match match in Regex.Matches(html, "<input\\b[^>]*>", RegexOptions.IgnoreCase))
        {
            var name = Regex.Match(match.Value, "name\\s*=\\s*\"([^\"]*)\"").Groups[1].Value;
            if (name.Length == 0)
                continue;

            var value = Regex.Match(match.Value, "value\\s*=\\s*\"([^\"]*)\"").Groups[1].Value;
            result[name] = value;
        }

        return result;
    }

    private static Task<HttpResponseMessage> GetAuthorizeAsync(HttpClient client, string clientId, string state, string? prompt = null)
    {
        var parameters = new Dictionary<string, string?>
        {
            ["client_id"] = clientId,
            ["redirect_uri"] = RedirectUri,
            ["response_type"] = "code",
            ["scope"] = Scope,
            ["code_challenge"] = CreateCodeChallenge(),
            ["code_challenge_method"] = "S256",
            ["state"] = state
        };

        if (prompt is not null)
            parameters["prompt"] = prompt;

        var query = string.Join('&', parameters.Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value!)}"));
        return client.GetAsync($"/connect/authorize?{query}");
    }

    private static string CreateCodeChallenge()
    {
        var verifier = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .TrimEnd('=').Replace('+', '-').Replace('/', '_');

        var challenge = Convert.ToBase64String(SHA256.HashData(Encoding.ASCII.GetBytes(verifier)))
            .TrimEnd('=').Replace('+', '-').Replace('/', '_');

        return challenge;
    }
}
