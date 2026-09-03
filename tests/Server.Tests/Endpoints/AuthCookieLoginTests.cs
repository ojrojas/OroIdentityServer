// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using System.Net;
using OroIdentityServer.Server.Tests.Infrastructure;
using Xunit;

namespace OroIdentityServer.Server.Tests.Endpoints;

/// <summary>
/// Integration tests (Aspire.Hosting.Testing) for the cookie-based session login at
/// POST /auth/login, executed against the full Aspire AppHost environment
/// (Postgres + RabbitMQ containers and the IdentityServer project).
/// </summary>
[Collection(nameof(AspireTestCollection))]
public sealed class AuthCookieLoginTests(AspireIdentityServerApp app)
{
    private const string AdminUser = "admin";
    private const string AdminPassword = "Admin@123456";

    [Fact]
    public async Task Login_WithValidCredentials_RedirectsAndSetsAuthCookie()
    {
        var client = app.CreateClient();

        var response = await client.PostAsync(
            "/auth/login",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["loginIdentifier"] = AdminUser,
                ["password"] = AdminPassword
            }));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/", response.Headers.Location?.OriginalString);

        var setCookie = GetSetCookieHeader(response);
        Assert.Contains("oro.identity.admin", setCookie);

        // The same HttpClient must now carry the session cookie on /api calls.
        var users = await client.GetAsync("/api/users");
        Assert.Equal(HttpStatusCode.OK, users.StatusCode);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_RedirectsToErrorWithoutAuthCookie()
    {
        var client = app.CreateClient();

        var response = await client.PostAsync(
            "/auth/login",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["loginIdentifier"] = AdminUser,
                ["password"] = "WrongPassword!"
            }));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.StartsWith("/Account/Login?error=invalid", response.Headers.Location?.OriginalString);

        var setCookie = GetSetCookieHeader(response);
        Assert.DoesNotContain("oro.identity.admin", setCookie);

        // Without a session cookie the API stays protected.
        var users = await client.GetAsync("/api/users");
        Assert.Equal(HttpStatusCode.Unauthorized, users.StatusCode);
    }

    private static string GetSetCookieHeader(HttpResponseMessage response)
        => response.Headers.TryGetValues("Set-Cookie", out var values)
            ? string.Join("; ", values)
            : string.Empty;
}
