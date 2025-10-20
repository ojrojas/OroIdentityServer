// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Services.OroIdentityServer.Api.Extensions;

public static class ApplicationExtensions
{
    public static TBuilder AddServiceApplication<TBuilder>(
        this TBuilder builder, IConfiguration configuration) where TBuilder : IHostApplicationBuilder
    {
        builder.AddServicesWritersLogger(configuration);
        
        builder.AddNpgsqlDbContext<OroIdentityAppContext>("identitydb", configureDbContextOptions: config =>
        {
           
        });

        return builder;
    }
}