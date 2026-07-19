using System.Reflection;
using BuildingBlocks.CQRS.Abstractions;
using BuildingBlocks.CQRS.Behaviors;
using BuildingBlocks.CQRS.Dispatchers;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.CQRS.DependencyInjection;

public static class CqrsServiceCollectionExtensions
{
    public static IServiceCollection AddBuildingBlocksCqrs(this IServiceCollection services, params Assembly[] assemblies)
    {
        services.AddScoped<ICommandDispatcher, CommandDispatcher>();
        services.AddScoped<IQueryDispatcher, QueryDispatcher>();

        var targets = assemblies.Length == 0 ? new[] { Assembly.GetCallingAssembly() } : assemblies;
        RegisterImplementations(services, targets, typeof(ICommandHandler<>));
        RegisterImplementations(services, targets, typeof(ICommandHandler<,>));
        RegisterImplementations(services, targets, typeof(IQueryHandler<,>));
        RegisterImplementations(services, targets, typeof(IValidator<>));
        return services;
    }

    public static IServiceCollection AddLoggingBehavior(this IServiceCollection services)
    {
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        return services;
    }

    public static IServiceCollection AddValidationBehavior(this IServiceCollection services)
    {
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        return services;
    }

    private static void RegisterImplementations(IServiceCollection services, Assembly[] assemblies, Type openGeneric)
    {
        foreach (var assembly in assemblies)
        {
            foreach (var type in assembly.GetTypes().Where(t => t is { IsClass: true, IsAbstract: false }))
            {
                foreach (var iface in type.GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == openGeneric))
                {
                    services.AddTransient(iface, type);
                }
            }
        }
    }
}
