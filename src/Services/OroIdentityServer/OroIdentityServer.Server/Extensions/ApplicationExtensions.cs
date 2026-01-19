// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Services.OroIdentityServer.Server.Extensions;

public static class ApplicationExtensions
{
    public static TBuilder AddServiceApplication<TBuilder>(
        this TBuilder builder, IConfiguration configuration) where TBuilder : IHostApplicationBuilder
    {
        builder.AddServicesWritersLogger(configuration);
        
        builder.AddNpgsqlDbContext<OroIdentityAppContext>("identitydb", configureDbContextOptions: config =>
        {
           config.UseNpgsql();
           config.UseOpenIddict();
           config.EnableDetailedErrors(); // Consider disabling in production for performance reasons
           config.EnableSensitiveDataLogging(); // Consider disabling in production for security reasons
        });

        builder.Services.AddProblemDetails();

        return builder;
    }
}