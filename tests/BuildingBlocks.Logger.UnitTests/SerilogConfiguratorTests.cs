using BuildingBlocks.Logger;
using BuildingBlocks.Logger.Options;
using FluentAssertions;
using Serilog;
using Serilog.Events;

namespace BuildingBlocks.Logger.UnitTests;

public sealed class SerilogConfiguratorTests
{
    [Fact]
    public void Configure_produces_logger_at_requested_level()
    {
        var options = new LoggerOptions
        {
            ApplicationName = "Tests",
            MinimumLevel = "Debug",
            Console = new ConsoleSinkOptions { Enabled = true },
            File = new FileSinkOptions { Enabled = false },
            Loki = new LokiSinkOptions { Enabled = false },
            Seq = new SeqSinkOptions { Enabled = false }
        };

        ILogger logger = SerilogConfigurator.Configure(new LoggerConfiguration(), options).CreateLogger();

        logger.IsEnabled(LogEventLevel.Debug).Should().BeTrue();
        logger.IsEnabled(LogEventLevel.Verbose).Should().BeFalse();
    }

    [Fact]
    public void Configure_throws_for_null_arguments()
    {
        var actNullCfg = () => SerilogConfigurator.Configure(null!, new LoggerOptions());
        actNullCfg.Should().Throw<ArgumentNullException>();
        var actNullOpts = () => SerilogConfigurator.Configure(new LoggerConfiguration(), null!);
        actNullOpts.Should().Throw<ArgumentNullException>();
    }
}
