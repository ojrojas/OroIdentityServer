// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using System.Net;
using Aspire.Hosting;
using Aspire.Hosting.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OroIdentityServer.Core.Interfaces;
using OroIdentityServer.Core.Services;
using OroIdentityServer.Infraestructure;
using Xunit;

namespace OroIdentityServer.Server.Tests.Infrastructure;

/// <summary>
/// Boots the whole Aspire AppHost (Postgres + RabbitMQ containers and the IdentityServer
/// project) once per test collection, exposing a pre-configured <see cref="HttpClient"/>
/// against the running server and a direct DbContext to the same Postgres database for seeding.
/// </summary>
public sealed class AspireIdentityServerApp : IAsyncLifetime
{
    private DistributedApplication? _app;
    private string _identityBaseUrl = null!;
    private string _postgresConnectionString = null!;

    public IPasswordHasher PasswordHasher { get; } = new PasswordHasher();

    public async Task InitializeAsync()
    {
        // Run the event bus in-memory for tests (no broker dependency), and force a non-Development
        // environment so the AppHost runs the IdentityServer project from source instead of the
        // prebuilt container image. Only the resources the API actually needs are started
        // (Postgres + identity-api): pgAdmin, RabbitMQ and the Angular admin SPA are skipped,
        // and Postgres uses ephemeral storage so every run initializes a fresh cluster that
        // matches the credentials Aspire generates for this run.
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.AppHost>(
            [
                "EventBus:Mode=InMemory",
                "--environment=Testing",
                "Resources:RabbitMQ=false",
                "Resources:PgAdmin=false",
                "Resources:IdentityAdmin=false",
                "Resources:PostgresDataVolume=false"
            ]);

        appHost.Services.AddLogging(logging => logging
            .AddConsole() // Outputs logs to console
            .AddFilter("Default", LogLevel.Information)
            .AddFilter("Microsoft.AspNetCore", LogLevel.Warning)
            .AddFilter("Aspire.Hosting.Dcp", LogLevel.Warning));

        _app = await appHost.BuildAsync();
        await _app.StartAsync();

        _identityBaseUrl = ResolveIdentityUrl();
        _postgresConnectionString = (await _app.GetConnectionStringAsync("identitydb"))!;

        await WaitUntilReadyAsync();
    }

    public HttpClient CreateClient()
    {
        var handler = new HttpClientHandler
        {
            AllowAutoRedirect = false,
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };

        var client = new HttpClient(handler)
        {
            BaseAddress = new Uri(_identityBaseUrl),
            Timeout = TimeSpan.FromSeconds(120)
        };
        return client;
    }

    public OroIdentityAppContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<OroIdentityAppContext>()
            .UseNpgsql(_postgresConnectionString)
            .Options;
        return new OroIdentityAppContext(options);
    }

    public async Task DisposeAsync()
    {
        if (_app is not null) await _app.DisposeAsync();
    }

    private string ResolveIdentityUrl()
    {
        try
        {
            return _app!.GetEndpoint("identity-api", "https").ToString();
        }
        catch (Exception)
        {
            return _app!.GetEndpoint("identity-api", "http").ToString();
        }
    }

    private async Task WaitUntilReadyAsync()
    {
        using var client = CreateClient();
        client.Timeout = TimeSpan.FromSeconds(5);
        // Cold starts (fresh ephemeral Postgres initdb + project build) can take several
        // minutes on slower machines or container runtimes.
        var deadline = DateTime.UtcNow.AddMinutes(5);
        while (DateTime.UtcNow < deadline)
        {
            try
            {
                using var response = await client.GetAsync("/");
                if (response.StatusCode is not (HttpStatusCode.ServiceUnavailable or HttpStatusCode.BadGateway))
                    return;
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or IOException)
            {
                // The web server is not accepting connections yet (still building/migrating/seeding).
            }

            await Task.Delay(1000);
        }

        throw new TimeoutException("The IdentityServer did not become ready in time.");
    }
}

[CollectionDefinition(nameof(AspireTestCollection))]
public sealed class AspireTestCollection : ICollectionFixture<AspireIdentityServerApp>
{
}
