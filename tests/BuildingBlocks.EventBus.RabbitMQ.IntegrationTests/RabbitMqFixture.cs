using Testcontainers.RabbitMq;

namespace BuildingBlocks.EventBus.RabbitMQ.IntegrationTests;

public sealed class RabbitMqFixture : IAsyncLifetime
{
    private RabbitMqContainer? _container;
    public string HostName { get; private set; } = "localhost";
    public int Port { get; private set; }
    public string UserName { get; private set; } = "guest";
    public string Password { get; private set; } = "guest";

    public bool DockerAvailable { get; private set; }

    public async Task InitializeAsync()
    {
        try
        {
            _container = new RabbitMqBuilder("rabbitmq:3.13-management")
                .WithUsername("guest")
                .WithPassword("guest")
                .Build();
            await _container.StartAsync();
            HostName = _container.Hostname;
            Port = _container.GetMappedPublicPort(5672);
            UserName = "guest";
            Password = "guest";
            DockerAvailable = true;
        }
        catch
        {
            DockerAvailable = false;
        }
    }

    public async Task DisposeAsync()
    {
        if (_container is not null) await _container.DisposeAsync();
    }
}

[CollectionDefinition(nameof(RabbitMqCollection))]
public sealed class RabbitMqCollection : ICollectionFixture<RabbitMqFixture> { }
