using BuildingBlocks.EventBus;
using BuildingBlocks.EventBus.RabbitMQ;
using BuildingBlocks.EventBus.RabbitMQ.DependencyInjection;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.EventBus.RabbitMQ.IntegrationTests;

[Collection(nameof(RabbitMqCollection))]
public sealed class RabbitMQEventBusTests
{
    private readonly RabbitMqFixture _fixture;
    public RabbitMQEventBusTests(RabbitMqFixture fixture) => _fixture = fixture;

    public sealed record OrderShipped(Guid OrderId) : IntegrationEvent;
    public sealed class OrderShippedHandler : IIntegrationEventHandler<OrderShipped>
    {
        public static TaskCompletionSource<OrderShipped> Tcs { get; } = new();
        public Task HandleAsync(OrderShipped @event, CancellationToken ct = default)
        {
            Tcs.TrySetResult(@event);
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task Publish_then_subscribe_delivers_event_to_handler()
    {
        if (!_fixture.DockerAvailable)
            return;

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["EventBus:RabbitMQ:HostName"] = _fixture.HostName,
                ["EventBus:RabbitMQ:Port"] = _fixture.Port.ToString(),
                ["EventBus:RabbitMQ:UserName"] = _fixture.UserName,
                ["EventBus:RabbitMQ:Password"] = _fixture.Password,
                ["EventBus:RabbitMQ:ExchangeName"] = "tests_exchange",
                ["EventBus:RabbitMQ:QueueName"] = $"tests_queue_{Guid.NewGuid():N}"
            }).Build();

        var services = new ServiceCollection();
        services.AddLogging(b => b.AddConsole().SetMinimumLevel(LogLevel.Warning));
        services.AddRabbitMQEventBus(config, handlerAssemblies: typeof(RabbitMQEventBusTests).Assembly);
        var sp = services.BuildServiceProvider();

        var bus = sp.GetRequiredService<IEventBus>();
        await bus.SubscribeAsync<OrderShipped, OrderShippedHandler>();
        var sent = new OrderShipped(Guid.NewGuid());
        await bus.PublishAsync(sent);

        var delivered = await OrderShippedHandler.Tcs.Task.WaitAsync(TimeSpan.FromSeconds(15));
        delivered.OrderId.Should().Be(sent.OrderId);
    }
}

