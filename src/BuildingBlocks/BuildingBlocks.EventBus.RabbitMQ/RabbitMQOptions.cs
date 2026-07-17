namespace BuildingBlocks.EventBus.RabbitMQ;

public sealed class RabbitMQOptions
{
    public const string SectionName = "EventBus:RabbitMQ";

    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string VirtualHost { get; set; } = "/";
    public string ExchangeName { get; set; } = "buildingblocks_event_bus";
    public string QueueName { get; set; } = "buildingblocks_default_queue";
    public int RetryCount { get; set; } = 5;
    public ushort PrefetchCount { get; set; } = 10;
    public bool Durable { get; set; } = true;
    public bool AutoDelete { get; set; }
}
