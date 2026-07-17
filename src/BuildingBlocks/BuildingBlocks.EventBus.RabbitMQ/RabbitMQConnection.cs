using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace BuildingBlocks.EventBus.RabbitMQ;

public sealed class RabbitMQConnection : IRabbitMQConnection
{
    private readonly IConnectionFactory _factory;
    private readonly ILogger<RabbitMQConnection> _logger;
    private readonly RabbitMQOptions _options;
    private readonly SemaphoreSlim _gate = new(1, 1);

    private IConnection? _connection;
    private bool _disposed;

    public RabbitMQConnection(IOptions<RabbitMQOptions> options, ILogger<RabbitMQConnection> logger)
    {
        _options = options.Value;
        _logger = logger;
        _factory = new ConnectionFactory
        {
            HostName = _options.HostName,
            Port = _options.Port,
            UserName = _options.UserName,
            Password = _options.Password,
            VirtualHost = _options.VirtualHost
        };
    }

    public bool IsConnected => _connection is { IsOpen: true } && !_disposed;

    public async Task<bool> TryConnectAsync(CancellationToken cancellationToken = default)
    {
        if (IsConnected) return true;

        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (IsConnected) return true;

            AsyncRetryPolicy policy = Policy
                .Handle<BrokerUnreachableException>()
                .Or<IOException>()
                .WaitAndRetryAsync(
                    _options.RetryCount,
                    attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                    (ex, time) => _logger.LogWarning(ex, "RabbitMQ connection failed (retry in {Delay})", time));

            await policy.ExecuteAsync(async () =>
            {
                _connection = await _factory.CreateConnectionAsync(cancellationToken).ConfigureAwait(false);
            }).ConfigureAwait(false);

            return IsConnected;
        }
        finally { _gate.Release(); }
    }

    public async Task<IChannel> CreateChannelAsync(CancellationToken cancellationToken = default)
    {
        if (!IsConnected) await TryConnectAsync(cancellationToken).ConfigureAwait(false);
        if (_connection is null) throw new InvalidOperationException("RabbitMQ connection is not available.");
        return await _connection.CreateChannelAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;
        if (_connection is not null)
            await _connection.DisposeAsync().ConfigureAwait(false);
        _gate.Dispose();
    }
}
