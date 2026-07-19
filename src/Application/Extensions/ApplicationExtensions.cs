// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using OroIdentityServer.Application.Modules.Tenants.Services;

namespace OroIdentityServer.Application.Extensions;

public static class ApplicationExtensions
{
    public static void AddApplicationExtensions(this IHostApplicationBuilder builder, IConfiguration configuration)
    {
        builder.Services.AddScoped<ITenantSchemaProvisioner, TenantSchemaProvisioner>();
        builder.Services.AddBuildingBlocksCqrs(typeof(ApplicationExtensions).Assembly);
        builder.Services.AddRabbitMQEventBus(
            configuration,
            options => options.ExchangeName = "oroeventdrivenexchange",
            typeof(ApplicationExtensions).Assembly);
    }
}