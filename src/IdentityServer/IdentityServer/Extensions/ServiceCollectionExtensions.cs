// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using System.Reflection;
using BuildingBlocks.CQRS.DependencyInjection;

namespace IdentityServer.Server.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers Application-layer CQRS command/query handlers without pulling
    /// in the RabbitMQ event bus (which the existing AddApplicationExtensions does).
    /// </summary>
    public static IServiceCollection AddApplicationHandlers(this IServiceCollection services)
    {
        services.AddBuildingBlocksCqrs(typeof(OroIdentityServer.Application.Extensions.ApplicationExtensions).Assembly);
       
        return services;
    }
}
