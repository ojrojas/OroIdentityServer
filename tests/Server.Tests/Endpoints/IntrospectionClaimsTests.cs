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
/// Introspection is how an external resource server asks "is this token still good, and what does
/// it say?". OpenIddict only releases the application claims (role, permission, tenant_id) to a
/// client that is an explicit audience of the token; to everyone else it either marks the token
/// inactive or answers with the protocol claims only. These tests pin both halves of that contract,
/// because the documented integration pattern - read the "permission" claim from introspection -
/// only works in the first case.
/// </summary>
[Collection(nameof(AspireTestCollection))]
public sealed class IntrospectionClaimsTests(AspireIdentityServerApp app)
{
    private const string AdminUser = "admin";
    private const string AdminPassword = "Admin@123456";

    [Fact]
    public async Task Introspection_ReturnsTheApplicationClaims_ToAnyClientGrantedTheEndpointPermission()
    {
        using var admin = await CreateLoggedInClientAsync();

        // The resource server is a registered client allowed to introspect. It is deliberately NOT
        // an audience of the token: this server returns the claims to any client that holds
        // ept:introspection, so the documented pattern does not also require registering the
        // resource server as a resource of every scope the frontend asks for.
        var resourceServer = $"rs-{Guid.NewGuid():N}";
        var resourceServerSecret = $"rs-secret-{Guid.NewGuid():N}";
        var scopeName = $"api-{Guid.NewGuid():N}";
        await CreateScopeAsync(admin, scopeName, [resourceServer]);
        await CreateResourceServerClientAsync(admin, resourceServer, resourceServerSecret);

        var spaId = $"spa-{Guid.NewGuid():N}";
        await CreatePublicClientAsync(admin, spaId, scopeName);

        var token = await RunAuthorizationCodeFlowAsync(admin, spaId, $"openid profile email roles permissions {scopeName}");

        var payload = await IntrospectAsync(admin, resourceServer, resourceServerSecret, token);

        Assert.True(payload.GetProperty("active").GetBoolean(), $"The token must be active. Response: {payload}");
        Assert.True(Guid.TryParse(payload.GetProperty("sub").GetString(), out _),
            "An interactive token must identify a user, not a client.");

        // The application claims the integration guide tells resource servers to read.
        Assert.Contains("Administrator", ReadStringOrArray(payload, "role"));
        Assert.Contains("system.*.*", ReadStringOrArray(payload, "permission"));
        Assert.False(string.IsNullOrWhiteSpace(payload.GetProperty("tenant_id").GetString()),
            "The tenant must be available so the resource server can scope its data.");

        // And the protocol claims the built-in handler already produces must survive.
        Assert.False(string.IsNullOrWhiteSpace(payload.GetProperty("iss").GetString()));
        Assert.False(string.IsNullOrWhiteSpace(payload.GetProperty("jti").GetString()));
        Assert.True(payload.GetProperty("exp").GetInt64() > 0);
        Assert.Equal("access_token", payload.GetProperty("token_usage").GetString());
    }

    [Fact]
    public async Task Introspection_RefusesAClientWithoutTheEndpointPermission_ForAResolvableToken()
    {
        using var admin = await CreateLoggedInClientAsync();
        var resourceServer = $"rs-{Guid.NewGuid():N}";
        var scopeName = $"api-{Guid.NewGuid():N}";
        await CreateScopeAsync(admin, scopeName, [resourceServer]);
        await CreateResourceServerClientAsync(admin, resourceServer, $"rs-secret-{Guid.NewGuid():N}");

        var spaId = $"spa-{Guid.NewGuid():N}";
        await CreatePublicClientAsync(admin, spaId, scopeName);
        var token = await RunAuthorizationCodeFlowAsync(admin, spaId, $"openid roles permissions {scopeName}");

        // A confidential client that is authenticated but was never granted ept:introspection.
        var outsider = $"no-introspection-{Guid.NewGuid():N}";
        var outsiderSecret = $"outsider-secret-{Guid.NewGuid():N}";
        await CreateClientAsync(admin, outsider, outsiderSecret, ["ept:token"]);

        var response = await admin.PostAsync("/connect/introspect", new FormUrlEncodedContent(
            new Dictionary<string, string>
            {
                ["client_id"] = outsider,
                ["client_secret"] = outsiderSecret,
                ["token"] = token,
                ["token_type_hint"] = TokenTypeHints.AccessToken
            }));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using var payload = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("unauthorized_client", payload.RootElement.GetProperty("error").GetString());
    }

    [Fact]
    public async Task Introspection_NeverLeaksTheApplicationClaims_ToAClientWithoutThePermission()
    {
        using var admin = await CreateLoggedInClientAsync();
        var resourceServer = $"rs-{Guid.NewGuid():N}";
        var scopeName = $"api-{Guid.NewGuid():N}";
        await CreateScopeAsync(admin, scopeName, [resourceServer]);
        await CreateResourceServerClientAsync(admin, resourceServer, $"rs-secret-{Guid.NewGuid():N}");

        var spaId = $"spa-{Guid.NewGuid():N}";
        await CreatePublicClientAsync(admin, spaId, scopeName);
        var token = await RunAuthorizationCodeFlowAsync(admin, spaId, $"openid roles permissions {scopeName}");

        var outsider = $"no-introspection-{Guid.NewGuid():N}";
        var outsiderSecret = $"outsider-secret-{Guid.NewGuid():N}";
        await CreateClientAsync(admin, outsider, outsiderSecret, ["ept:token"]);

        // A token that cannot be resolved short-circuits inside OpenIddict before the endpoint
        // permission is checked, so this answers 200 - but it must still carry nothing.
        var response = await admin.PostAsync("/connect/introspect", new FormUrlEncodedContent(
            new Dictionary<string, string>
            {
                ["client_id"] = outsider,
                ["client_secret"] = outsiderSecret,
                ["token"] = "not-a-token",
                ["token_type_hint"] = TokenTypeHints.AccessToken
            }));

        using var payload = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.False(payload.RootElement.GetProperty("active").GetBoolean());
        Assert.False(payload.RootElement.TryGetProperty("permission", out _));
        Assert.False(payload.RootElement.TryGetProperty("role", out _));

        // The fetched-but-unauthorised token is what actually matters: the live token above must not
        // have been disclosed either.
        Assert.DoesNotContain(token, await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Introspection_ReportsAnInactiveTokenAsInactive()
    {
        using var admin = await CreateLoggedInClientAsync();
        var resourceServer = $"rs-{Guid.NewGuid():N}";
        var secret = $"rs-secret-{Guid.NewGuid():N}";
        var scopeName = $"api-{Guid.NewGuid():N}";
        await CreateScopeAsync(admin, scopeName, [resourceServer]);
        await CreateResourceServerClientAsync(admin, resourceServer, secret);

        var spaId = $"spa-{Guid.NewGuid():N}";
        await CreatePublicClientAsync(admin, spaId, scopeName);
        var token = await RunAuthorizationCodeFlowAsync(admin, spaId, $"openid {scopeName}");

        var before = await IntrospectAsync(admin, resourceServer, secret, token);
        Assert.True(before.GetProperty("active").GetBoolean());

        var after = await IntrospectAsync(admin, resourceServer, secret, "this-is-not-a-token");
        Assert.False(after.GetProperty("active").GetBoolean(),
            "A token that cannot be resolved must be reported as inactive.");
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

    private static async Task CreateScopeAsync(HttpClient client, string name, IReadOnlyList<string> resources)
    {
        var response = await client.PostAsJsonAsync("/api/scopes", new { name, resources });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    private static async Task CreateResourceServerClientAsync(HttpClient client, string clientId, string clientSecret)
        => await CreateClientAsync(client, clientId, clientSecret, [Permissions.Endpoints.Introspection]);

    private static async Task CreateClientAsync(
        HttpClient client, string clientId, string clientSecret, IReadOnlyList<string> permissions)
    {
        var response = await client.PostAsJsonAsync("/api/applications", new
        {
            clientId,
            clientSecret,
            displayName = "Confidential client",
            clientType = ClientTypes.Confidential,
            applicationType = ApplicationTypes.Web,
            consentType = ConsentTypes.Implicit,
            permissions,
            requirements = Array.Empty<string>(),
            redirectUris = Array.Empty<string>(),
            postLogoutRedirectUris = Array.Empty<string>()
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    /// <summary>A public PKCE client granted the given extra scope, standing in for the SPA.</summary>
    private static async Task CreatePublicClientAsync(HttpClient client, string clientId, string grantedScope)
    {
        var response = await client.PostAsJsonAsync("/api/applications", new
        {
            clientId,
            clientSecret = (string?)null,
            displayName = "Introspection SPA",
            clientType = ClientTypes.Public,
            applicationType = ApplicationTypes.Web,
            consentType = ConsentTypes.Implicit,
            permissions = new[]
            {
                Permissions.Endpoints.Authorization, Permissions.Endpoints.Token,
                Permissions.GrantTypes.AuthorizationCode, Permissions.ResponseTypes.Code,
                "scp:openid", "scp:profile", "scp:email", "scp:roles", "scp:permissions",
                $"scp:{grantedScope}"
            },
            requirements = new[] { Requirements.Features.ProofKeyForCodeExchange },
            redirectUris = new[] { RedirectUri },
            postLogoutRedirectUris = new[] { "https://localhost/introspection-test" }
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    private static async Task<string> RunAuthorizationCodeFlowAsync(HttpClient client, string clientId, string scope)
    {
        var verifier = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .TrimEnd('=').Replace('+', '-').Replace('/', '_');
        var challenge = Convert.ToBase64String(SHA256.HashData(Encoding.ASCII.GetBytes(verifier)))
            .TrimEnd('=').Replace('+', '-').Replace('/', '_');

        var authorize = await client.GetAsync(
            $"/connect/authorize?client_id={Uri.EscapeDataString(clientId)}" +
            $"&redirect_uri={Uri.EscapeDataString(RedirectUri)}" +
            $"&response_type=code&scope={Uri.EscapeDataString(scope)}" +
            $"&code_challenge={challenge}&code_challenge_method=S256" +
            $"&state=state-{Guid.NewGuid():N}");

        Assert.Equal(HttpStatusCode.Redirect, authorize.StatusCode);
        var code = ParseQueryParameter(authorize.Headers.Location!.OriginalString, "code");

        var tokenResponse = await client.PostAsync("/connect/token", new FormUrlEncodedContent(
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
        return payload.RootElement.GetProperty("access_token").GetString()!;
    }

    private static async Task<JsonElement> IntrospectAsync(
        HttpClient client, string clientId, string clientSecret, string token)
    {
        var response = await client.PostAsync("/connect/introspect", new FormUrlEncodedContent(
            new Dictionary<string, string>
            {
                ["client_id"] = clientId,
                ["client_secret"] = clientSecret,
                ["token"] = token,
                ["token_type_hint"] = TokenTypeHints.AccessToken
            }));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return document.RootElement.Clone();
    }

    private static IEnumerable<string?> ReadStringOrArray(JsonElement payload, string property)
    {
        if (!payload.TryGetProperty(property, out var value))
            return [];

        return value.ValueKind == JsonValueKind.Array
            ? value.EnumerateArray().Select(item => item.GetString())
            : [value.GetString()];
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

    private const string RedirectUri = "https://localhost/introspection-test/callback";
}
