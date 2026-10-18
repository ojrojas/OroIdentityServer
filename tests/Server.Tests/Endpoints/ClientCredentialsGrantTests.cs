// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3 or later.
// See the LICENSE file in the project root for details.
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using static OpenIddict.Abstractions.OpenIddictConstants;
using OroIdentityServer.Server.Tests.Infrastructure;
using Xunit;

namespace OroIdentityServer.Server.Tests.Endpoints;

/// <summary>
/// Integration tests (Aspire.Hosting.Testing) for the client_credentials grant: a confidential
/// client exchanges its own secret for a token. There is no resource owner, so RFC 6749 §4.4 makes
/// this a machine-to-machine flow: the granted scopes and resources decide what the token can
/// reach, and consent - an authorization-endpoint concern - plays no part.
/// </summary>
[Collection(nameof(AspireTestCollection))]
public sealed class ClientCredentialsGrantTests(AspireIdentityServerApp app)
{
    private const string AdminUser = "admin";
    private const string AdminPassword = "Admin@123456";

    [Theory]
    // All three consent types must behave identically: this grant never visits the authorization
    // endpoint, so an explicit-consent client must not be pushed into a consent screen nobody can
    // approve.
    [InlineData(ConsentTypes.Implicit)]
    [InlineData(ConsentTypes.Explicit)]
    [InlineData(ConsentTypes.External)]
    public async Task ClientCredentials_IssuesATokenForEveryConsentType(string consentType)
    {
        using var client = await CreateLoggedInClientAsync();
        var machine = new MachineClient($"m2m-{Guid.NewGuid():N}", $"m2m-secret-{Guid.NewGuid():N}");
        await CreateScopeAsync(client, machine.ScopeName, [machine.ScopeName, machine.ClientId]);
        await CreateMachineClientAsync(client, machine, consentType, includeGrant: true);

        var response = await RequestTokenAsync(client, machine, machine.ScopeName);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var payload = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        Assert.Equal("Bearer", payload.RootElement.GetProperty("token_type").GetString());
        Assert.False(string.IsNullOrWhiteSpace(payload.RootElement.GetProperty("access_token").GetString()));

        // No user is involved, so there is no identity token.
        Assert.False(payload.RootElement.TryGetProperty("id_token", out _),
            "client_credentials must not issue an id_token: there is no end user to identify.");

        // RFC 6749 §4.4.3: a refresh token SHOULD NOT be issued. The client can already mint a new
        // token from its own credentials, so one would only add a long-lived secret to leak.
        Assert.False(payload.RootElement.TryGetProperty("refresh_token", out _),
            "client_credentials must not issue a refresh_token.");
    }

    [Fact]
    public async Task ClientCredentials_ExplicitConsent_DoesNotDemandConsent()
    {
        // Regression guard: if the grant were ever routed through the authorization endpoint, an
        // explicit-consent client would answer consent_required.
        using var client = await CreateLoggedInClientAsync();
        var machine = new MachineClient($"m2m-{Guid.NewGuid():N}", $"m2m-secret-{Guid.NewGuid():N}");
        await CreateScopeAsync(client, machine.ScopeName, [machine.ScopeName]);
        await CreateMachineClientAsync(client, machine, ConsentTypes.Explicit, includeGrant: true);

        var response = await RequestTokenAsync(client, machine, machine.ScopeName);

        var body = await response.Content.ReadAsStringAsync();
        Assert.True(response.StatusCode == HttpStatusCode.OK,
            $"client_credentials must never demand consent. Got {(int)response.StatusCode}: {body}");

        using var payload = JsonDocument.Parse(body);
        Assert.False(payload.RootElement.TryGetProperty("error", out var error),
            $"client_credentials must never demand consent. Got: {error}");
    }

    [Fact]
    public async Task ClientCredentials_TokenExposesScopesAndAudienceButNoUserEntitlements()
    {
        using var client = await CreateLoggedInClientAsync();
        var machine = new MachineClient($"m2m-{Guid.NewGuid():N}", $"m2m-secret-{Guid.NewGuid():N}");
        var resource = $"m2m-api-{Guid.NewGuid():N}";

        // The machine's own client id is registered as a resource of the scope, so the token lists
        // it as an audience. That matters: OpenIddict only returns application claims to a client
        // that is an explicit audience, so without this the "no user entitlements" assertions below
        // would pass for the wrong reason (withheld, not absent).
        await CreateScopeAsync(client, machine.ScopeName, [resource, machine.ClientId]);
        await CreateMachineClientAsync(client, machine, ConsentTypes.Implicit, includeGrant: true);

        var token = await RequestAccessTokenAsync(client, machine, machine.ScopeName);

        var payload = await IntrospectAsync(client, machine, token);

        Assert.True(payload.GetProperty("active").GetBoolean(), $"The M2M token must be active. Response: {payload}");
        Assert.Equal(machine.ClientId, payload.GetProperty("sub").GetString());
        Assert.Contains(machine.ScopeName, payload.GetProperty("scope").GetString()!.Split(' '));
        Assert.Contains(resource, ReadAudiences(payload));

        // A machine token must never carry a human's entitlements: those are derived from the
        // user's catalogue roles and permissions, and there is no user in this grant. Because the
        // client is an audience, these would be present if the grant had leaked them.
        Assert.False(payload.TryGetProperty("role", out _), "A machine token must not carry user roles.");
        Assert.False(payload.TryGetProperty("permission", out _), "A machine token must not carry user permissions.");
        Assert.False(payload.TryGetProperty("tenant_id", out _), "A machine token must not carry a user's tenant.");
        Assert.False(payload.TryGetProperty("is_master_admin", out _), "A machine token must not be a master admin.");
    }

    [Fact]
    public async Task ClientCredentials_Token_CannotReachTheAdminApi()
    {
        // The admin API authorises on roles (ManagerOrAdmin / MasterAdminOnly), which a machine
        // token deliberately lacks. This is the security boundary of the whole change. The answer is
        // 403, not 401: the token authenticates fine, it simply is not authorised.
        using var admin = await CreateLoggedInClientAsync();
        var machine = new MachineClient($"m2m-{Guid.NewGuid():N}", $"m2m-secret-{Guid.NewGuid():N}");
        await CreateScopeAsync(admin, machine.ScopeName, [machine.ScopeName]);
        await CreateMachineClientAsync(admin, machine, ConsentTypes.Implicit, includeGrant: true);
        var token = await RequestAccessTokenAsync(admin, machine, machine.ScopeName);

        // A cookie-less client on purpose: reusing the logged-in admin client would send its admin
        // cookie alongside the bearer token and the cookie would win, hiding the boundary.
        using var anonymous = app.CreateClient();
        foreach (var path in new[] { "/api/users", "/api/applications" })
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, path);
            request.Headers.Authorization = new("Bearer", token);
            var response = await anonymous.SendAsync(request);

            var body = await response.Content.ReadAsStringAsync();
            Assert.True(response.StatusCode == HttpStatusCode.Forbidden,
                $"{path} must reject a machine token with 403. Got {(int)response.StatusCode}: {body}");
        }
    }

    [Fact]
    public async Task ClientCredentials_Token_GetsNoSelfServiceData()
    {
        // /api/me/mfa only requires an authenticated caller, so a machine token does authenticate -
        // but the service resolves the caller from the user id claim and there is no user. It answers
        // with its own ApiResponse envelope (HTTP 200, statusCode 401) rather than an HTTP-level
        // denial, so the assertion is that no data comes back, not that the status is 403.
        using var admin = await CreateLoggedInClientAsync();
        var machine = new MachineClient($"m2m-{Guid.NewGuid():N}", $"m2m-secret-{Guid.NewGuid():N}");
        await CreateScopeAsync(admin, machine.ScopeName, [machine.ScopeName]);
        await CreateMachineClientAsync(admin, machine, ConsentTypes.Implicit, includeGrant: true);
        var token = await RequestAccessTokenAsync(admin, machine, machine.ScopeName);

        using var anonymous = app.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/me/mfa");
        request.Headers.Authorization = new("Bearer", token);
        var response = await anonymous.SendAsync(request);

        using var payload = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.True(payload.RootElement.TryGetProperty("data", out var data) && data.ValueKind == JsonValueKind.Null,
            $"A machine token must not be given a user's MFA state. Response: {payload.RootElement}");
        Assert.Equal(401, payload.RootElement.GetProperty("statusCode").GetInt32());
    }

    [Fact]
    public async Task ClientCredentials_Token_HasNoUserToDescribe()
    {
        // /connect/userinfo is the user-profile endpoint; a machine token has no profile to return,
        // and answering with anything but an error would invent an identity.
        using var admin = await CreateLoggedInClientAsync();
        var machine = new MachineClient($"m2m-{Guid.NewGuid():N}", $"m2m-secret-{Guid.NewGuid():N}");
        await CreateScopeAsync(admin, machine.ScopeName, [machine.ScopeName]);
        await CreateMachineClientAsync(admin, machine, ConsentTypes.Implicit, includeGrant: true);
        var token = await RequestAccessTokenAsync(admin, machine, machine.ScopeName);

        using var anonymous = app.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/connect/userinfo");
        request.Headers.Authorization = new("Bearer", token);
        var response = await anonymous.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ClientCredentials_WithoutTheGrantPermission_IsRejected()
    {
        using var client = await CreateLoggedInClientAsync();
        var machine = new MachineClient($"m2m-{Guid.NewGuid():N}", $"m2m-secret-{Guid.NewGuid():N}");
        await CreateScopeAsync(client, machine.ScopeName, [machine.ScopeName]);
        await CreateMachineClientAsync(client, machine, ConsentTypes.Implicit, includeGrant: false);

        var response = await RequestTokenAsync(client, machine, machine.ScopeName);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using var payload = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("unauthorized_client", payload.RootElement.GetProperty("error").GetString());
    }

    [Fact]
    public async Task ClientCredentials_WithAWrongSecret_IsRejected()
    {
        using var client = await CreateLoggedInClientAsync();
        var machine = new MachineClient($"m2m-{Guid.NewGuid():N}", $"m2m-secret-{Guid.NewGuid():N}");
        await CreateScopeAsync(client, machine.ScopeName, [machine.ScopeName]);
        await CreateMachineClientAsync(client, machine, ConsentTypes.Implicit, includeGrant: true);

        var response = await RequestTokenAsync(client, machine with { ClientSecret = "not-the-secret" }, machine.ScopeName);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        using var payload = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("invalid_client", payload.RootElement.GetProperty("error").GetString());
    }

    [Fact]
    public async Task ClientCredentials_WithARegisteredScopeTheClientWasNotGranted_IsRejected()
    {
        // OpenIddict answers "invalid_request" (ID2051) when the scope exists but the client was not
        // granted it, and "invalid_scope" (ID2052) when the scope is not registered at all - see the
        // sibling test. Both are 400s; pinning the exact code is what keeps them distinguishable.
        using var client = await CreateLoggedInClientAsync();
        var machine = new MachineClient($"m2m-{Guid.NewGuid():N}", $"m2m-secret-{Guid.NewGuid():N}");
        await CreateScopeAsync(client, machine.ScopeName, [machine.ScopeName]);

        var ungrantedScope = $"ungranted-{Guid.NewGuid():N}";
        await CreateScopeAsync(client, ungrantedScope, [ungrantedScope]);
        await CreateMachineClientAsync(client, machine, ConsentTypes.Implicit, includeGrant: true);

        var response = await RequestTokenAsync(client, machine, ungrantedScope);

        var body = await response.Content.ReadAsStringAsync();
        Assert.True(response.StatusCode == HttpStatusCode.BadRequest,
            $"An ungranted scope must be rejected. Got {(int)response.StatusCode}: {body}");

        using var payload = JsonDocument.Parse(body);
        Assert.Equal("invalid_request", payload.RootElement.GetProperty("error").GetString());
    }

    [Fact]
    public async Task ClientCredentials_WithAScopeThatDoesNotExist_IsRejected()
    {
        using var client = await CreateLoggedInClientAsync();
        var machine = new MachineClient($"m2m-{Guid.NewGuid():N}", $"m2m-secret-{Guid.NewGuid():N}");
        await CreateScopeAsync(client, machine.ScopeName, [machine.ScopeName]);
        await CreateMachineClientAsync(client, machine, ConsentTypes.Implicit, includeGrant: true);

        var response = await RequestTokenAsync(client, machine, $"never-registered-{Guid.NewGuid():N}");

        var body = await response.Content.ReadAsStringAsync();
        Assert.True(response.StatusCode == HttpStatusCode.BadRequest,
            $"An unknown scope must be rejected. Got {(int)response.StatusCode}: {body}");

        using var payload = JsonDocument.Parse(body);
        Assert.Equal("invalid_scope", payload.RootElement.GetProperty("error").GetString());
    }

    // ----- helpers -------------------------------------------------------------------------

    /// <summary>
    /// A machine client under test. <see cref="ScopeName"/> is materialised once: as a computed
    /// property it would hand out a fresh GUID on every read, so the scope registered and the scope
    /// requested would never be the same one.
    /// </summary>
    private sealed record MachineClient(string ClientId, string ClientSecret)
    {
        public string ScopeName { get; } = $"m2m-api-{Guid.NewGuid():N}";
    }

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

    private static async Task CreateMachineClientAsync(
        HttpClient client, MachineClient machine, string consentType, bool includeGrant)
    {
        var permissions = new List<string>
        {
            Permissions.Endpoints.Token,
            // The machine introspects its own tokens in these tests, which is the only client
            // OpenIddict will report on (see IntrospectAsync).
            Permissions.Endpoints.Introspection,
            $"scp:{machine.ScopeName}"
        };

        // Permissions.GrantTypes is the "gt:"-prefixed form stored on the application; the bare
        // GrantTypes.ClientCredentials value is what a request puts in the "grant_type" parameter,
        // and registering that instead silently grants nothing.
        if (includeGrant) permissions.Add(Permissions.GrantTypes.ClientCredentials);

        var response = await client.PostAsJsonAsync("/api/applications", new
        {
            clientId = machine.ClientId,
            clientSecret = machine.ClientSecret,
            displayName = "Machine client",
            clientType = ClientTypes.Confidential,
            applicationType = ApplicationTypes.Web,
            consentType,
            permissions,
            requirements = Array.Empty<string>(),
            redirectUris = Array.Empty<string>(),
            postLogoutRedirectUris = Array.Empty<string>()
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    private static Task<HttpResponseMessage> RequestTokenAsync(
        HttpClient client, MachineClient machine, string scope)
        => client.PostAsync("/connect/token", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = GrantTypes.ClientCredentials,
            ["client_id"] = machine.ClientId,
            ["client_secret"] = machine.ClientSecret,
            ["scope"] = scope
        }));

    private static async Task<string> RequestAccessTokenAsync(
        HttpClient client, MachineClient machine, string scope)
    {
        var response = await RequestTokenAsync(client, machine, scope);
        var body = await response.Content.ReadAsStringAsync();
        Assert.True(response.StatusCode == HttpStatusCode.OK,
            $"Expected a client_credentials token. Got {(int)response.StatusCode}: {body}");

        using var payload = JsonDocument.Parse(body);
        var token = payload.RootElement.GetProperty("access_token").GetString();
        Assert.False(string.IsNullOrWhiteSpace(token));
        return token!;
    }

    /// <summary>
    /// Introspects with the machine's own credentials: OpenIddict refuses to report a token to an
    /// unrelated client, so a third-party introspector would always see <c>active: false</c>.
    /// </summary>
    private static async Task<JsonElement> IntrospectAsync(HttpClient client, MachineClient machine, string token)
    {
        var response = await client.PostAsync("/connect/introspect", new FormUrlEncodedContent(
            new Dictionary<string, string>
            {
                ["client_id"] = machine.ClientId,
                ["client_secret"] = machine.ClientSecret,
                ["token"] = token,
                ["token_type_hint"] = TokenTypeHints.AccessToken
            }));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return document.RootElement.Clone();
    }

    private static IEnumerable<string?> ReadAudiences(JsonElement payload)
    {
        if (!payload.TryGetProperty("aud", out var audience))
            return [];

        return audience.ValueKind == JsonValueKind.Array
            ? audience.EnumerateArray().Select(value => value.GetString())
            : [audience.GetString()];
    }
}
