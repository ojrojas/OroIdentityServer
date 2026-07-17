// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using BuildingBlocks.EventBus;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using Xunit;

namespace OroIdentityServer.Server.Tests.Infrastructure;

internal sealed class StubEventBus : IEventBus
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

public sealed class IdentityServerWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine")
        .WithDatabase("identitydb")
        .WithUsername("postgres")
        .WithPassword("Weak(!)Password123")
        .Build();

    public async Task InitializeAsync() => await _postgres.StartAsync();
    public new async Task DisposeAsync() => await _postgres.DisposeAsync();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:identitydb", _postgres.GetConnectionString());
        builder.UseSetting("DatabaseSeeder:Skip", "true");
        builder.ConfigureServices(services =>
        {
            services.AddSingleton<IEventBus, StubEventBus>();
        });
    }
}
