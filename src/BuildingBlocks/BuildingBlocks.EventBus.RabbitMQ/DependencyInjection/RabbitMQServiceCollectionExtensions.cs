using System.Reflection;
using BuildingBlocks.EventBus.Subscriptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.EventBus.RabbitMQ.DependencyInjection;

public static class RabbitMQServiceCollectionExtensions
{
    public const string InMemoryMode = "InMemory";
    public const string RabbitMQMode = "RabbitMQ";

    public static IServiceCollection AddEventBus(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<RabbitMQOptions>? configure = null,
        params Assembly[] handlerAssemblies)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var mode = configuration["EventBus:Mode"] ?? InMemoryMode;
        if (string.Equals(mode, RabbitMQMode, StringComparison.OrdinalIgnoreCase))
            return services.AddRabbitMQEventBus(configuration, configure, handlerAssemblies);

        RegisterEventBusCore(services, handlerAssemblies.Length == 0 ? [Assembly.GetCallingAssembly()] : handlerAssemblies);
        services.AddScoped<IEventBus, InMemoryEventBus>();
        return services;
    }

    public static IServiceCollection AddRabbitMQEventBus(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<RabbitMQOptions>? configure = null,
        params Assembly[] handlerAssemblies)
    {
        services.AddOptions<RabbitMQOptions>()
            .Bind(configuration.GetSection(RabbitMQOptions.SectionName))
            .Configure(o => configure?.Invoke(o));

        RegisterEventBusCore(services, handlerAssemblies.Length == 0 ? [Assembly.GetCallingAssembly()] : handlerAssemblies);
        services.AddScoped<IRabbitMQConnection, RabbitMQConnection>();
        services.AddScoped<IEventBus, RabbitMQEventBus>();
        return services;
    }

    private static void RegisterEventBusCore(IServiceCollection services, Assembly[] handlerAssemblies)
    {
        services.AddScoped<ISubscriptionRegistry, InMemorySubscriptionRegistry>();

        var openHandler = typeof(IIntegrationEventHandler<>);
        foreach (var assembly in handlerAssemblies)
        {
            foreach (var type in assembly.GetTypes().Where(t => t is { IsClass: true, IsAbstract: false }))
            {
                foreach (var iface in type.GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == openHandler))
                {
                    services.AddScoped(iface, type);
                    services.AddScoped(type);
                }
            }
        }
    }
}
