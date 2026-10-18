// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3 or later.
// See the LICENSE file in the project root for details.
using System.Net;
using System.Text.Json;
using static OpenIddict.Abstractions.OpenIddictConstants;
using OroIdentityServer.Server.Tests.Infrastructure;
using Xunit;

namespace OroIdentityServer.Server.Tests.Endpoints;

/// <summary>
/// The two applications the seeder ships as examples: the same SPA registered twice, once with
/// implicit consent and once with explicit consent. The difference is entirely in what
/// /connect/authorize does after the user is signed in, so that is what these tests pin - it is the
/// behaviour someone reading the examples is trying to understand.
/// </summary>
[Collection(nameof(AspireTestCollection))]
public sealed class SeededExampleClientsTests(AspireIdentityServerApp app)
{
    private const string AdminUser = "admin";
    private const string AdminPassword = "Admin@123456";

    /// <summary>
    /// With no <c>IDENTITY_ADMIN_HTTP</c> supplied (as in this test host) the seeder falls back to
    /// the conventional dev-server origins, which is what these redirect URIs come from.
    /// </summary>
    private const string ImplicitRedirectUri = "http://localhost:4200/auth/callback";
    private const string ExplicitRedirectUri = "http://localhost:4201/auth/callback";

    /// <summary>Any valid S256 challenge: no code is exchanged in these tests.</summary>
    private const string CodeChallenge = "E9Melhoa2OwvFrEMTJguCHaoeK1t8URWbuGJSstw-cM";

    [Fact]
    public async Task SeededExamples_UseDifferentConsentTypes()
    {
        using var client = await CreateLoggedInClientAsync();

        Assert.Equal(ConsentTypes.Implicit, await ReadConsentTypeAsync(client, "OroIdentityServer.Admin"));
        Assert.Equal(ConsentTypes.Explicit, await ReadConsentTypeAsync(client, "OroIdentityServer.Admin.Explicit"));
    }

    [Fact]
    public async Task ImplicitExample_ReturnsTheCodeWithoutAConsentScreen()
    {
        using var client = await CreateLoggedInClientAsync();

        var response = await AuthorizeAsync(client, "OroIdentityServer.Admin", ImplicitRedirectUri);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        var location = response.Headers.Location!.OriginalString;
        Assert.StartsWith(ImplicitRedirectUri, location);
        Assert.Contains("code=", location);
        Assert.DoesNotContain("/Account/Consent", location);
    }

    [Fact]
    public async Task ExplicitExample_DetoursThroughTheConsentScreen()
    {
        using var client = await CreateLoggedInClientAsync();

        var response = await AuthorizeAsync(client, "OroIdentityServer.Admin.Explicit", ExplicitRedirectUri);

        // The user is already signed in, so this is purely the consent step: it does not log them in
        // again, it asks for approval of the scopes this resource may use.
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.StartsWith("/Account/Consent?", response.Headers.Location!.OriginalString);
    }

    [Fact]
    public async Task ConsentScreen_OffersAcceptAndDeny()
    {
        using var client = await CreateLoggedInClientAsync();
        await AuthorizeAsync(client, "OroIdentityServer.Admin.Explicit", ExplicitRedirectUri);

        var page = await client.GetAsync(
            $"/Account/Consent?client_id=OroIdentityServer.Admin.Explicit&response_type=code&scope=openid%20profile%20roles");

        Assert.Equal(HttpStatusCode.OK, page.StatusCode);
        var html = await page.Content.ReadAsStringAsync();

        // The form posts back the two decisions the controller implements.
        Assert.Contains("submit.Accept", html);
        Assert.Contains("submit.Deny", html);
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

    private static async Task<string?> ReadConsentTypeAsync(HttpClient client, string clientId)
    {
        var response = await client.GetAsync($"/api/applications/{clientId}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return document.RootElement.GetProperty("consentType").GetString();
    }

    private static Task<HttpResponseMessage> AuthorizeAsync(HttpClient client, string clientId, string redirectUri)
        => client.GetAsync(
            $"/connect/authorize?client_id={Uri.EscapeDataString(clientId)}" +
            $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
            "&response_type=code&scope=openid%20profile%20email%20roles" +
            $"&code_challenge={CodeChallenge}&code_challenge_method=S256" +
            $"&state=state-{Guid.NewGuid():N}");
}
