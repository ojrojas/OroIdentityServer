using System.Net;
using System.Net.Http.Json;
using BuildingBlocks.EventBus;
using BuildingBlocks.Examples.OrdersApi.Modules.Orders.Slices;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BuildingBlocks.Examples.OrdersApi.IntegrationTests;

public sealed class OrdersEndpointsTests : IClassFixture<OrdersApiFactory>
{
    private readonly OrdersApiFactory _factory;
    public OrdersEndpointsTests(OrdersApiFactory factory) => _factory = factory;

    [Fact]
    public async Task POST_orders_creates_and_GET_returns_it()
    {
        var client = _factory.CreateClient();

        var create = await client.PostAsJsonAsync("/orders", new CreateOrder("Alice", 99.5m, "USD"));
        create.StatusCode.Should().Be(HttpStatusCode.Created);

        var payload = await create.Content.ReadFromJsonAsync<CreatedResponse>();
        payload.Should().NotBeNull();

        var get = await client.GetAsync($"/orders/{payload!.Id}");
        get.IsSuccessStatusCode.Should().BeTrue();
        var dto = await get.Content.ReadFromJsonAsync<OrderDto>();
        dto.Should().NotBeNull();
        dto!.Customer.Should().Be("Alice");
        dto.Amount.Should().Be(99.5m);
        dto.Currency.Should().Be("USD");
    }

    [Fact]
    public async Task POST_orders_with_invalid_payload_returns_400()
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/orders", new CreateOrder("", -1, "X"));
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GET_unknown_order_returns_404()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync($"/orders/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Health_endpoints_are_mapped()
    {
        var client = _factory.CreateClient();
        (await client.GetAsync("/health/live")).IsSuccessStatusCode.Should().BeTrue();
        (await client.GetAsync("/health/ready")).IsSuccessStatusCode.Should().BeTrue();
    }

    private sealed record CreatedResponse(Guid Id);
}

public sealed class OrdersApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Production");
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IEventBus>();
            services.AddSingleton<IEventBus, NoopEventBus>();
        });
    }
}

internal sealed class NoopEventBus : IEventBus
{
    public Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : IntegrationEvent
        => Task.CompletedTask;

    public Task SubscribeAsync<TEvent, THandler>(CancellationToken cancellationToken = default)
        where TEvent : IntegrationEvent
        where THandler : IIntegrationEventHandler<TEvent>
        => Task.CompletedTask;

    public Task UnsubscribeAsync<TEvent, THandler>(CancellationToken cancellationToken = default)
        where TEvent : IntegrationEvent
        where THandler : IIntegrationEventHandler<TEvent>
        => Task.CompletedTask;
}
