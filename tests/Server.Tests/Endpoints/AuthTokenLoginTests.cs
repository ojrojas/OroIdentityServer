// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
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
/// Integration tests (Aspire.Hosting.Testing) for the OpenID Connect token login:
/// the full authorization code + PKCE (S256) flow against /connect/authorize and
/// /connect/token, executed on the Aspire AppHost.
/// </summary>
[Collection(nameof(AspireTestCollection))]
public sealed class AuthTokenLoginTests(AspireIdentityServerApp app)
{
    private const string AdminUser = "admin";
    private const string AdminPassword = "Admin@123456";
    private const string RedirectUri = "https://localhost/token-login-test/callback";
    private const string Scope = "openid profile email roles";

    [Fact]
    public async Task TokenLogin_WithAuthorizationCodeAndPkce_IssuesTokens()
    {
        var client = await CreateLoggedInClientAsync();
        var clientId = await CreatePublicClientAsync(client);
        var (verifier, challenge) = CreatePkce();

        var code = await RequestAuthorizationCodeAsync(client, clientId, challenge);

        var tokenResponse = await ExchangeCodeAsync(client, clientId, code, verifier);

        Assert.Equal(HttpStatusCode.OK, tokenResponse.StatusCode);
        using var payload = JsonDocument.Parse(await tokenResponse.Content.ReadAsStringAsync());
        Assert.False(string.IsNullOrWhiteSpace(payload.RootElement.GetProperty("access_token").GetString()));
        Assert.False(string.IsNullOrWhiteSpace(payload.RootElement.GetProperty("id_token").GetString()));
        Assert.Equal("Bearer", payload.RootElement.GetProperty("token_type").GetString());
    }

    [Fact]
    public async Task TokenLogin_WithWrongCodeVerifier_RejectsGrant()
    {
        var client = await CreateLoggedInClientAsync();
        var clientId = await CreatePublicClientAsync(client);
        var (verifier, challenge) = CreatePkce();

        var code = await RequestAuthorizationCodeAsync(client, clientId, challenge);

        // Tamper with the PKCE verifier so it no longer matches the sent challenge.
        var tamperedVerifier = verifier[..^2] + "xx";
        var tokenResponse = await ExchangeCodeAsync(client, clientId, code, tamperedVerifier);

        Assert.Equal(HttpStatusCode.BadRequest, tokenResponse.StatusCode);
        using var payload = JsonDocument.Parse(await tokenResponse.Content.ReadAsStringAsync());
        Assert.Equal("invalid_grant", payload.RootElement.GetProperty("error").GetString());
    }

    /// <summary>Cookie login first: the authorize endpoint requires an authenticated session.</summary>
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

    /// <summary>
    /// Registers a public OIDC application with implicit consent through the admin API,
    /// so the authorize step redirects straight back with a code (no consent screen).
    /// </summary>
    private static async Task<string> CreatePublicClientAsync(HttpClient client)
    {
        var clientId = $"token-login-{Guid.NewGuid():N}";
        var payload = new
        {
            clientId,
            clientSecret = (string?)null,
            displayName = "Token Login Test App",
            clientType = ClientTypes.Public,
            applicationType = ApplicationTypes.Web,
            consentType = ConsentTypes.Implicit,
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
                "scp:roles"
            },
            requirements = new[] { "ft:pkce" },
            redirectUris = new[] { RedirectUri },
            postLogoutRedirectUris = new[] { "https://localhost/token-login-test" }
        };

        var response = await client.PostAsJsonAsync("/api/applications", payload);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return clientId;
    }

    private static async Task<string> RequestAuthorizationCodeAsync(HttpClient client, string clientId, string challenge)
    {
        var parameters = new Dictionary<string, string?>
        {
            ["client_id"] = clientId,
            ["redirect_uri"] = RedirectUri,
            ["response_type"] = "code",
            ["scope"] = Scope,
            ["code_challenge"] = challenge,
            ["code_challenge_method"] = "S256",
            ["state"] = $"state-{Guid.NewGuid():N}"
        };

        var query = string.Join('&', parameters.Select(
            p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value!)}"));
        var authorize = await client.GetAsync($"/connect/authorize?{query}");

        Assert.Equal(HttpStatusCode.Redirect, authorize.StatusCode);
        var location = authorize.Headers.Location!.OriginalString;
        Assert.StartsWith(RedirectUri, location);

        return ParseQueryParameter(location, "code");
    }

    private static Task<HttpResponseMessage> ExchangeCodeAsync(HttpClient client, string clientId, string code, string codeVerifier)
        => client.PostAsync("/connect/token", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "authorization_code",
            ["code"] = code,
            ["code_verifier"] = codeVerifier,
            ["client_id"] = clientId,
            ["redirect_uri"] = RedirectUri
        }));

    private static (string Verifier, string Challenge) CreatePkce()
    {
        var verifier = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .TrimEnd('=').Replace('+', '-').Replace('/', '_');

        var challenge = Convert.ToBase64String(SHA256.HashData(Encoding.ASCII.GetBytes(verifier)))
            .TrimEnd('=').Replace('+', '-').Replace('/', '_');

        return (verifier, challenge);
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
