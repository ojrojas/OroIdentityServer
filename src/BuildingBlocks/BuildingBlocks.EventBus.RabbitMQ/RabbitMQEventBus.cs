using System.Text;
using System.Text.Json;
using BuildingBlocks.EventBus.Subscriptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace BuildingBlocks.EventBus.RabbitMQ;

public sealed class RabbitMQEventBus : IEventBus, IAsyncDisposable
{
    private readonly IRabbitMQConnection _connection;
    private readonly ISubscriptionRegistry _registry;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RabbitMQEventBus> _logger;
    private readonly RabbitMQOptions _options;
    private IChannel? _consumerChannel;

    public RabbitMQEventBus(
        IRabbitMQConnection connection,
        ISubscriptionRegistry registry,
        IServiceScopeFactory scopeFactory,
        IOptions<RabbitMQOptions> options,
        ILogger<RabbitMQEventBus> logger)
    {
        _connection = connection;
        _registry = registry;
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
        _registry.OnEventRemoved += OnEventRemoved;
    }

    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IntegrationEvent
    {
        ArgumentNullException.ThrowIfNull(@event);
        if (!await _connection.TryConnectAsync(cancellationToken).ConfigureAwait(false))
            throw new InvalidOperationException("Cannot publish: RabbitMQ connection unavailable.");

        var policy = Policy
            .Handle<BrokerUnreachableException>()
            .Or<IOException>()
            .WaitAndRetryAsync(
                _options.RetryCount,
                attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                (ex, t) => _logger.LogWarning(ex, "Publish retry in {Delay}", t));

        await using var channel = await _connection.CreateChannelAsync(cancellationToken).ConfigureAwait(false);
        await channel.ExchangeDeclareAsync(
            exchange: _options.ExchangeName,
            type: ExchangeType.Direct,
            durable: _options.Durable,
            autoDelete: _options.AutoDelete,
            cancellationToken: cancellationToken).ConfigureAwait(false);

        var routingKey = @event.EventName;
        var body = JsonSerializer.SerializeToUtf8Bytes((object)@event, @event.GetType());
        var props = new BasicProperties
        {
            DeliveryMode = DeliveryModes.Persistent,
            MessageId = @event.EventId.ToString(),
            Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds()),
            Type = routingKey
        };

        await policy.ExecuteAsync(async () =>
        {
            await channel.BasicPublishAsync(
                exchange: _options.ExchangeName,
                routingKey: routingKey,
                mandatory: false,
                basicProperties: props,
                body: body,
                cancellationToken: cancellationToken).ConfigureAwait(false);
        }).ConfigureAwait(false);

        _logger.LogInformation("Published integration event {EventName} ({EventId})", routingKey, @event.EventId);
    }

    public async Task SubscribeAsync<TEvent, THandler>(CancellationToken cancellationToken = default)
        where TEvent : IntegrationEvent
        where THandler : IIntegrationEventHandler<TEvent>
    {
        var key = _registry.GetEventKey<TEvent>();
        await EnsureConsumerChannelAsync(cancellationToken).ConfigureAwait(false);
        await _consumerChannel!.QueueBindAsync(
            queue: _options.QueueName,
            exchange: _options.ExchangeName,
            routingKey: key,
            cancellationToken: cancellationToken).ConfigureAwait(false);

        _registry.Add<TEvent, THandler>();
        await StartBasicConsumeAsync(cancellationToken).ConfigureAwait(false);
        _logger.LogInformation("Subscribed handler {Handler} to event {EventName}", typeof(THandler).Name, key);
    }

    public Task UnsubscribeAsync<TEvent, THandler>(CancellationToken cancellationToken = default)
        where TEvent : IntegrationEvent
        where THandler : IIntegrationEventHandler<TEvent>
    {
        _registry.Remove<TEvent, THandler>();
        return Task.CompletedTask;
    }

    private async Task EnsureConsumerChannelAsync(CancellationToken cancellationToken)
    {
        if (_consumerChannel is { IsOpen: true }) return;
        _consumerChannel = await _connection.CreateChannelAsync(cancellationToken).ConfigureAwait(false);

        await _consumerChannel.ExchangeDeclareAsync(
            exchange: _options.ExchangeName,
            type: ExchangeType.Direct,
            durable: _options.Durable,
            autoDelete: _options.AutoDelete,
            cancellationToken: cancellationToken).ConfigureAwait(false);

        await _consumerChannel.QueueDeclareAsync(
            queue: _options.QueueName,
            durable: _options.Durable,
            exclusive: false,
            autoDelete: _options.AutoDelete,
            cancellationToken: cancellationToken).ConfigureAwait(false);

        await _consumerChannel.BasicQosAsync(0, _options.PrefetchCount, false, cancellationToken).ConfigureAwait(false);
    }

    private async Task StartBasicConsumeAsync(CancellationToken cancellationToken)
    {
        if (_consumerChannel is null) return;
        var consumer = new AsyncEventingBasicConsumer(_consumerChannel);
        consumer.ReceivedAsync += OnMessageReceivedAsync;
        await _consumerChannel.BasicConsumeAsync(
            queue: _options.QueueName,
            autoAck: false,
            consumer: consumer,
            cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    private async Task OnMessageReceivedAsync(object sender, BasicDeliverEventArgs ea)
    {
        var eventName = ea.RoutingKey;
        var payload = Encoding.UTF8.GetString(ea.Body.Span);
        try
        {
            var eventType = _registry.GetEventType(eventName);
            if (eventType is null)
            {
                _logger.LogWarning("No registered type for event {EventName}", eventName);
            }
            else
            {
                var @event = JsonSerializer.Deserialize(payload, eventType);
                using var scope = _scopeFactory.CreateScope();
                foreach (var sub in _registry.GetHandlersFor(eventName))
                {
                    var handler = scope.ServiceProvider.GetService(sub.HandlerType);
                    if (handler is null) continue;
                    var concreteHandler = typeof(IIntegrationEventHandler<>).MakeGenericType(eventType);
                    var method = concreteHandler.GetMethod(nameof(IIntegrationEventHandler<IntegrationEvent>.HandleAsync))!;
                    await ((Task)method.Invoke(handler, new[] { @event!, CancellationToken.None })!).ConfigureAwait(false);
                }
            }
            await _consumerChannel!.BasicAckAsync(ea.DeliveryTag, multiple: false).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling integration event {EventName}", eventName);
            await _consumerChannel!.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true).ConfigureAwait(false);
        }
    }

    private void OnEventRemoved(object? sender, string eventName)
    {
        if (_consumerChannel is null) return;
        _ = _consumerChannel.QueueUnbindAsync(
            queue: _options.QueueName,
            exchange: _options.ExchangeName,
            routingKey: eventName);
    }

    public async ValueTask DisposeAsync()
    {
        if (_consumerChannel is not null)
            await _consumerChannel.DisposeAsync().ConfigureAwait(false);
    }
}
