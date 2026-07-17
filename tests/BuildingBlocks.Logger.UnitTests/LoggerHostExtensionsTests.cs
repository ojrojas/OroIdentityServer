using BuildingBlocks.Logger.DependencyInjection;
using BuildingBlocks.Logger.Options;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Logger.UnitTests;

public sealed class LoggerHostExtensionsTests
{
    [Fact]
    public void AddBuildingBlocksLogger_binds_configuration_and_overrides()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Logging:BuildingBlocks:ApplicationName"] = "FromConfig",
                ["Logging:BuildingBlocks:MinimumLevel"] = "Warning"
            }).Build();

        var services = new ServiceCollection();
        services.AddBuildingBlocksLogger(config, o => o.MinimumLevel = "Error");

        var sp = services.BuildServiceProvider();
        var options = sp.GetRequiredService<LoggerOptions>();
        options.ApplicationName.Should().Be("FromConfig");
        options.MinimumLevel.Should().Be("Error");
    }
}
