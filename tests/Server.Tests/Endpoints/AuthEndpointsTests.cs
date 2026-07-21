// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using System.Net;
using FluentAssertions;
using OroIdentityServer.Server.Tests.Infrastructure;
using Xunit;

namespace OroIdentityServer.Server.Tests.Endpoints;

public sealed class AuthEndpointsTests(IdentityServerWebApplicationFactory factory)
    : IClassFixture<IdentityServerWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient(new() { AllowAutoRedirect = false });

    [Fact]
    public async Task Login_WithInvalidCredentials_RedirectsBackToLoginWithError()
    {
        var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["loginIdentifier"] = "noone",
            ["password"] = "wrong"
        });

        var response = await _client.PostAsync("/auth/login", form);

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location!.OriginalString.Should().Contain("/Account/Login?error=invalid&ReturnUrl=");
    }
}
