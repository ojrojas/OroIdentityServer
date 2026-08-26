// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using System.Net;
using System.Net.Http.Json;
using IdentityServer.Client.Models;
using IdentityServer.Client.Models.Tenants;
using OroIdentityServer.Server.Tests.Infrastructure;
using Xunit;

namespace OroIdentityServer.Server.Tests.Endpoints;

/// <summary>
/// Integration tests (Aspire.Hosting.Testing) for the tenants catalogue endpoint
/// GET /api/tenants/ (MasterAdminOnly), executed against the full Aspire AppHost.
/// </summary>
[Collection(nameof(AspireTestCollection))]
public sealed class TenantsListTests(AspireIdentityServerApp app)
{
    private const string AdminUser = "admin";
    private const string AdminPassword = "Admin@123456";
    private const string MasterTenantName = "OroMasterTenant";

    [Fact]
    public async Task GetTenants_WithMasterAdminSession_ReturnsSeededTenant()
    {
        var client = await CreateLoggedInClientAsync();

        var response = await client.GetAsync("/api/tenants/");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<TenantModel>>>();
        Assert.NotNull(payload);
        Assert.NotNull(payload.Data);
        Assert.NotEmpty(payload.Data);
        Assert.Contains(payload.Data!, t => t.Name == MasterTenantName);
    }

    [Fact]
    public async Task GetTenants_WithoutSession_ReturnsUnauthorized()
    {
        var client = app.CreateClient();

        var response = await client.GetAsync("/api/tenants/");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("unauthorized", body);
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
}
