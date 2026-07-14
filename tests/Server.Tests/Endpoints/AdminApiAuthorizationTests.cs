// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using System.Net;
using FluentAssertions;
using OroIdentityServer.Server.Tests.Infrastructure;
using Xunit;

namespace OroIdentityServer.Server.Tests.Endpoints;

public sealed class AdminApiAuthorizationTests(IdentityServerWebApplicationFactory factory)
    : IClassFixture<IdentityServerWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient(new() { AllowAutoRedirect = false });

    [Theory]
    [InlineData("/api/users")]
    [InlineData("/api/roles")]
    [InlineData("/api/permissions")]
    [InlineData("/api/identification-types")]
    [InlineData("/api/applications")]
    [InlineData("/api/scopes")]
    public async Task ProtectedEndpoints_RequireAuth(string url)
    {
        var response = await _client.GetAsync(url);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Redirect, HttpStatusCode.Found);
    }
}
