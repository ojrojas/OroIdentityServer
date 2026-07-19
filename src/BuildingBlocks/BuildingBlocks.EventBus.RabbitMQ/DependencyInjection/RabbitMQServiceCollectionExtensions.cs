using System.Reflection;
using BuildingBlocks.EventBus.Subscriptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.EventBus.RabbitMQ.DependencyInjection;

public static class RabbitMQServiceCollectionExtensions
{
    public static IServiceCollection AddRabbitMQEventBus(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<RabbitMQOptions>? configure = null,
        params Assembly[] handlerAssemblies)
    {
        services.AddOptions<RabbitMQOptions>()
            .Bind(configuration.GetSection(RabbitMQOptions.SectionName))
            .Configure(o => configure?.Invoke(o));

        services.AddScoped<IRabbitMQConnection, RabbitMQConnection>();
        services.AddScoped<ISubscriptionRegistry, InMemorySubscriptionRegistry>();
        services.AddScoped<IEventBus, RabbitMQEventBus>();

        var assemblies = handlerAssemblies.Length == 0 ? new[] { Assembly.GetCallingAssembly() } : handlerAssemblies;
        var openHandler = typeof(IIntegrationEventHandler<>);
        foreach (var assembly in assemblies)
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

        return services;
    }
}
