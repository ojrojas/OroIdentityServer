// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.BuildingBlocks.Loggers;

public static class LoggersExtensions
{
    public static void AddServicesWritersLogger(
        this IHostApplicationBuilder builder, IConfiguration configuration)
    {
         builder.AddSeqEndpoint("seq");
        builder.Services.AddSerilog();
        builder.Services.AddLogging(options => options.AddSeq());
    }
}