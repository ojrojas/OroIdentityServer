// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3 or later.
// See the LICENSE file in the project root for details.
using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using static OpenIddict.Abstractions.OpenIddictConstants;
using OroIdentityServer.Server.Tests.Infrastructure;
using Xunit;

namespace OroIdentityServer.Server.Tests.Endpoints;

/// <summary>
/// Revocation has to be visible on both ways of reading a token, or the documented "remote logout"
/// stops working. Introspection has always consulted the token store; local validation did not, so
/// a token revoked by logout or by an admin kept authorising /api calls until it expired. These
/// tests pin that both paths now agree.
/// </summary>
[Collection(nameof(AspireTestCollection))]
public sealed class TokenRevocationTests(AspireIdentityServerApp app)
{
    private const string AdminUser = "admin";
    private const string AdminPassword = "Admin@123456";
    private const string RedirectUri = "https://localhost/revocation-test/callback";

    [Fact]
    public async Task TerminateAllSessions_InvalidatesTheAccessTokenOnTheApi()
    {
        using var admin = await CreateLoggedInClientAsync();
        var (spaId, token, subject) = await IssueUserTokenAsync(admin);

        // Before: the token is accepted and introspection agrees.
        var before = await CallWithTokenAsync("/api/users", token);
        Assert.Equal(HttpStatusCode.OK, before.StatusCode);
        Assert.True(await IsActiveAsync(admin, spaId, token));

        var terminated = await admin.PostAsync($"/api/user-sessions/terminate-all/{subject}", null);
        Assert.Equal(HttpStatusCode.NoContent, terminated.StatusCode);

        // After: the same token must stop working. This is the remote-logout contract.
        var after = await CallWithTokenAsync("/api/users", token);
        var body = await after.Content.ReadAsStringAsync();
        Assert.True(after.StatusCode == HttpStatusCode.Unauthorized,
            $"A revoked access token must be refused by /api with 401. Got {(int)after.StatusCode}: {body}");

        Assert.False(await IsActiveAsync(admin, spaId, token),
            "Introspection must report a revoked token as inactive.");
    }

    [Fact]
    public async Task RevokingASingleSession_AlsoInvalidatesTheAccessToken()
    {
        using var admin = await CreateLoggedInClientAsync();
        var (_, token, subject) = await IssueUserTokenAsync(admin);

        // GET /api/user-sessions/by-user is reachable from a plain HttpClient: it used to be mapped
        // to the QUERY verb only, which the Blazor console can send but a browser or HttpClient
        // cannot, so this used to 404.
        var sessions = await admin.GetAsync($"/api/user-sessions/by-user?userId={subject}");
        Assert.Equal(HttpStatusCode.OK, sessions.StatusCode);

        using var document = JsonDocument.Parse(await sessions.Content.ReadAsStringAsync());
        var sessionId = ReadFirstSessionId(document.RootElement, subject);

        Assert.False(string.IsNullOrEmpty(sessionId),
            "The signed-in admin must have an active session to revoke.");

        var deactivated = await admin.PostAsync($"/api/user-sessions/{sessionId}/deactivate", null);
        Assert.Equal(HttpStatusCode.NoContent, deactivated.StatusCode);

        var after = await CallWithTokenAsync("/api/users", token);
        Assert.Equal(HttpStatusCode.Unauthorized, after.StatusCode);
    }

    /// <summary>
    /// Reads the session id out of either response shape the endpoint can produce (a bare array, or
    /// an ApiResponse envelope), and only for the user under test.
    /// </summary>
    private static string? ReadFirstSessionId(JsonElement payload, string subject)
    {
        var items = payload.ValueKind == JsonValueKind.Array
            ? payload
            : payload.GetProperty("data");

        foreach (var item in items.EnumerateArray())
        {
            if (!item.TryGetProperty("userId", out var userId)
                || !string.Equals(userId.GetString(), subject, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            return item.GetProperty("id").GetString();
        }

        return null;
    }

    // ----- helpers -------------------------------------------------------------------------

    private async Task<HttpClient> CreateLoggedInClientAsync()
    {
        var client = app.CreateClient();
        var login = await client.PostAsync(
            "/auth/login",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["loginIdentifier"] = AdminUser,
                ["password"] = AdminPassword
            }));

        Assert.Equal(HttpStatusCode.Redirect, login.StatusCode);
        return client;
    }

    /// <summary>Runs the authorization code flow with a fresh public client and returns its token.</summary>
    private async Task<(string ClientId, string AccessToken, string Subject)> IssueUserTokenAsync(HttpClient admin)
    {
        var clientId = $"revocation-{Guid.NewGuid():N}";
        var created = await admin.PostAsJsonAsync("/api/applications", new
        {
            clientId,
            clientSecret = (string?)null,
            displayName = "Revocation test SPA",
            clientType = ClientTypes.Public,
            applicationType = ApplicationTypes.Web,
            consentType = ConsentTypes.Implicit,
            permissions = new[]
            {
                Permissions.Endpoints.Authorization, Permissions.Endpoints.Token,
                Permissions.Endpoints.Introspection,
                Permissions.GrantTypes.AuthorizationCode, Permissions.ResponseTypes.Code,
                "scp:openid", "scp:profile", "scp:roles"
            },
            requirements = new[] { Requirements.Features.ProofKeyForCodeExchange },
            redirectUris = new[] { RedirectUri },
            postLogoutRedirectUris = new[] { "https://localhost/revocation-test" }
        });
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);

        var verifier = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .TrimEnd('=').Replace('+', '-').Replace('/', '_');
        var challenge = Convert.ToBase64String(SHA256.HashData(Encoding.ASCII.GetBytes(verifier)))
            .TrimEnd('=').Replace('+', '-').Replace('/', '_');

        var authorize = await admin.GetAsync(
            $"/connect/authorize?client_id={Uri.EscapeDataString(clientId)}" +
            $"&redirect_uri={Uri.EscapeDataString(RedirectUri)}" +
            "&response_type=code&scope=openid%20profile%20roles" +
            $"&code_challenge={challenge}&code_challenge_method=S256" +
            $"&state=state-{Guid.NewGuid():N}");

        Assert.Equal(HttpStatusCode.Redirect, authorize.StatusCode);
        var code = ParseQueryParameter(authorize.Headers.Location!.OriginalString, "code");

        var tokenResponse = await admin.PostAsync("/connect/token", new FormUrlEncodedContent(
            new Dictionary<string, string>
            {
                ["grant_type"] = GrantTypes.AuthorizationCode,
                ["client_id"] = clientId,
                ["code"] = code,
                ["code_verifier"] = verifier,
                ["redirect_uri"] = RedirectUri
            }));

        var body = await tokenResponse.Content.ReadAsStringAsync();
        Assert.True(tokenResponse.StatusCode == HttpStatusCode.OK,
            $"The authorization code flow must succeed. Got {(int)tokenResponse.StatusCode}: {body}");

        using var payload = JsonDocument.Parse(body);
        return (clientId, payload.RootElement.GetProperty("access_token").GetString()!, ReadSubject(payload.RootElement));
    }

    /// <summary>The subject lives in the id_token, which is a plain JWS the test can read.</summary>
    private static string ReadSubject(JsonElement tokenPayload)
    {
        var idToken = tokenPayload.GetProperty("id_token").GetString()!;
        var segments = idToken.Split('.');
        var json = Encoding.UTF8.GetString(Base64UrlDecode(segments[1]));

        using var document = JsonDocument.Parse(json);
        return document.RootElement.GetProperty("sub").GetString()!;
    }

    private static byte[] Base64UrlDecode(string value)
    {
        var padded = value.Replace('-', '+').Replace('_', '/');
        padded += new string('=', (4 - padded.Length % 4) % 4);
        return Convert.FromBase64String(padded);
    }

    private async Task<HttpResponseMessage> CallWithTokenAsync(string path, string token)
    {
        // Cookie-less on purpose: the logged-in admin client would let its cookie answer first.
        using var anonymous = app.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, path);
        request.Headers.Authorization = new("Bearer", token);
        return await anonymous.SendAsync(request);
    }

    private async Task<bool> IsActiveAsync(HttpClient client, string introspectingClientId, string token)
    {
        var response = await client.PostAsync("/connect/introspect", new FormUrlEncodedContent(
            new Dictionary<string, string>
            {
                ["client_id"] = introspectingClientId,
                ["token"] = token,
                ["token_type_hint"] = TokenTypeHints.AccessToken
            }));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return document.RootElement.GetProperty("active").GetBoolean();
    }

    private static string ParseQueryParameter(string uri, string parameterName)
    {
        var query = uri[(uri.IndexOf('?') + 1)..];
        foreach (var pair in query.Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = pair.Split('=', 2);
            if (parts.Length == 2 && Uri.UnescapeDataString(parts[0]) == parameterName)
                return Uri.UnescapeDataString(parts[1]);
        }

        throw new InvalidOperationException($"Query parameter '{parameterName}' was not found in '{uri}'.");
    }
}
