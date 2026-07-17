using BuildingBlocks.ServicesDefaults;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;

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
        (await client.GetAsync("/health/live")).IsSuccessStatusCode.Should().BeTrue();
        (await client.GetAsync("/health/ready")).IsSuccessStatusCode.Should().BeTrue();

        await app.StopAsync();
    }
}
