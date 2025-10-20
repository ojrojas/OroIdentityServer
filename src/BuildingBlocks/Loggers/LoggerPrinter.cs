// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.BuildingBlocks.Loggers;

public static class LoggerPrinter
{
    public static Serilog.ILogger CreateSerilogLogger(string key, string value, IConfiguration configuration)
    {
        string? SeqEndpoint = configuration["Seq"];
        ArgumentNullException.ThrowIfNull(SeqEndpoint);
        var logger = new LoggerConfiguration()
              .MinimumLevel.Verbose()
              .Enrich.WithProperty(key, value)
              .Enrich.FromLogContext()
              .WriteTo.Console()
              .WriteTo.Seq(SeqEndpoint)
              .CreateLogger();

        Serilog.Debugging.SelfLog.Enable(Console.Error);

        return logger;
    }
}