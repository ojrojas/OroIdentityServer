using BuildingBlocks.Logger.Options;
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

        Assert.True(logger.IsEnabled(LogEventLevel.Debug));
        Assert.False(logger.IsEnabled(LogEventLevel.Verbose));
    }

    [Fact]
    public void Configure_throws_for_null_arguments()
    {
        var actNullCfg = () => SerilogConfigurator.Configure(null!, new LoggerOptions());
        Assert.Throws<ArgumentNullException>(actNullCfg);
        var actNullOpts = () => SerilogConfigurator.Configure(new LoggerConfiguration(), null!);
        Assert.Throws<ArgumentNullException>(actNullOpts);
    }
}
