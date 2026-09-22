// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using System.Net;
using System.Net.Http.Json;
using OroIdentityServer.Server.Tests.Infrastructure;
using Xunit;

namespace OroIdentityServer.Server.Tests.Endpoints;

/// <summary>
/// Regression coverage for listing OpenIddict applications. The application entity stores
/// <c>string[]</c> properties (Permissions, RedirectUris, ...) that OpenIddict 8 maps as native
/// PostgreSQL arrays; reading them must not throw the EF/Npgsql "Reading as 'System.String[]'"
/// InvalidCastException that occurs when the database columns are still JSON <c>text</c>.
/// </summary>
[Collection(nameof(AspireTestCollection))]
public sealed class ApplicationsListTests(AspireIdentityServerApp app)
{
    private const string AdminUser = "admin";
    private const string AdminPassword = "Admin@123456";

    [Fact]
    public async Task ListApplications_AsMasterAdmin_ReturnsOk()
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

        // Applications are listed with the custom QUERY method (mirrors the Blazor client).
        using var request = new HttpRequestMessage(HttpMethod.Query, "/api/applications")
        {
            Content = JsonContent.Create(new { SearchTerm = (string?)null, PageNumber = 1, PageSize = 10 })
        };
        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
