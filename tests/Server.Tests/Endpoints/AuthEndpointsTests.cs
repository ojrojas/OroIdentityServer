// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using System.Net;
using OroIdentityServer.Server.Tests.Infrastructure;
using Xunit;

namespace OroIdentityServer.Server.Tests.Endpoints;

[Collection(nameof(AspireTestCollection))]
public sealed class AuthEndpointsTests(AspireIdentityServerApp app)
{
    private readonly HttpClient _client = app.CreateClient();

    [Fact]
    public async Task Login_WithInvalidCredentials_RedirectsBackToLoginWithError()
    {
        var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["loginIdentifier"] = "noone",
            ["password"] = "wrong"
        });

        var response = await _client.PostAsync("/auth/login", form);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Account/Login?error=invalid&ReturnUrl=", response.Headers.Location!.OriginalString);
    }
}
