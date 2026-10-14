// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using System.Net;
using System.Net.Http.Json;
using IdentityServer.Client.Models;
using OroIdentityServer.Server.Tests.Infrastructure;
using Xunit;

namespace OroIdentityServer.Server.Tests.Endpoints;

[Collection(nameof(AspireTestCollection))]
public sealed class AdminApiAuthorizationTests(AspireIdentityServerApp app)
{
    private readonly HttpClient _client = app.CreateClient();

    [Theory]
    [InlineData("/api/users", "GET")]
    [InlineData("/api/roles", "QUERY")]
    [InlineData("/api/permissions", "GET")]
    [InlineData("/api/identification-types", "GET")]
    [InlineData("/api/applications", "QUERY")]
    [InlineData("/api/scopes", "GET")]
    public async Task ProtectedEndpoints_RequireAuth(string url, string method)
    {
        using var request = new HttpRequestMessage(new HttpMethod(method), url);
        if (method == "QUERY")
            request.Content = JsonContent.Create(new PagedRequest());

        var response = await _client.SendAsync(request);
        Assert.Contains(response.StatusCode, new[] { HttpStatusCode.Unauthorized, HttpStatusCode.Redirect, HttpStatusCode.Found });
    }
}
