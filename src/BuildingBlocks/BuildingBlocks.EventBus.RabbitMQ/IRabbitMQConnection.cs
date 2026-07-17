using RabbitMQ.Client;

namespace BuildingBlocks.EventBus.RabbitMQ;

public interface IRabbitMQConnection : IAsyncDisposable
{
    bool IsConnected { get; }
    Task<bool> TryConnectAsync(CancellationToken cancellationToken = default);
    Task<IChannel> CreateChannelAsync(CancellationToken cancellationToken = default);
}
