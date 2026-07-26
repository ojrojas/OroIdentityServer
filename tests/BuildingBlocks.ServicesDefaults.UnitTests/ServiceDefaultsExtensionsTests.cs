using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;

namespace BuildingBlocks.ServicesDefaults.UnitTests;

public sealed class ServiceDefaultsExtensionsTests
{
    [Fact]
    public async Task Default_health_endpoints_return_200()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.AddBuildingBlocksDefaults();
        var app = builder.Build();
        app.MapDefaultEndpoints();
        await app.StartAsync();

        var client = app.GetTestClient();
        Assert.True((await client.GetAsync("/health/live")).IsSuccessStatusCode);
        Assert.True((await client.GetAsync("/health/ready")).IsSuccessStatusCode);

        await app.StopAsync();
    }
}
