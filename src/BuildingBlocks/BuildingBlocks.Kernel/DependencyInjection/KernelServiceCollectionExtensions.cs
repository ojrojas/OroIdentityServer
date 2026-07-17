using System.Reflection;
using BuildingBlocks.Kernel.Events;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Kernel.DependencyInjection;

public static class KernelServiceCollectionExtensions
{
    public static IServiceCollection AddBuildingBlocksKernel(this IServiceCollection services, params Assembly[] assemblies)
    {
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

        var openHandler = typeof(IDomainEventHandler<>);
        var targets = assemblies.Length == 0 ? new[] { Assembly.GetCallingAssembly() } : assemblies;
        foreach (var assembly in targets)
        {
            foreach (var type in assembly.GetTypes().Where(t => t is { IsClass: true, IsAbstract: false }))
            {
                foreach (var iface in type.GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == openHandler))
                {
                    services.AddTransient(iface, type);
                }
            }
        }
        return services;
    }
}
