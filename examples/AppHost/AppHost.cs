IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

var environment = builder.Environment;

IResourceBuilder<RabbitMQServerResource> rabbitMq = builder.AddRabbitMQ("oroeventdrivenexchange")
    .WithLifetime(ContainerLifetime.Persistent);

IResourceBuilder<PostgresServerResource> postgres = builder.AddPostgres("postgres")
    .WithDataVolume("oro-postgres-data");

postgres.WithPgAdmin(container => container.WithImageTag("latest"));

IResourceBuilder<PostgresDatabaseResource> identityDb = postgres.AddDatabase("identitydb");

IResourceBuilder<ParameterResource> SymmetricSecurityKey = builder.AddParameter("SymmetricSecurityKey", "g9hLodrPUXAJRCxQUMZA6Bo2l8amqDjeHRerJIJAhVs=");

// Overridable from the AppHost configuration (e.g. "EventBus:Mode=InMemory" passed by tests),
// so tests can run without depending on the broker transport.
var eventBusMode = builder.Configuration["EventBus:Mode"] ?? "RabbitMQ";

// Runs the IdentityServer from source. Tests (Aspire.Hosting.Testing) and
// `aspire run` both execute the current code; `aspire publish` builds the
// container image from this project.
IResourceBuilder<ProjectResource> identityServer = builder.AddProject<Projects.IdentityServer>("identity-api")
    .WithHttpEndpoint(port: 5080, name: "http")
    .WithReference(rabbitMq).WaitFor(rabbitMq)
    .WithReference(identityDb).WaitFor(identityDb)
    .WithEnvironment("SEED_TENANT_NAME", "OroMasterTenant")
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", environment.EnvironmentName)
    .WithEnvironment("SymmetricSecurityKey", SymmetricSecurityKey)
    .WithEnvironment("EventBus__Mode", eventBusMode)
    .WithEnvironment("EventBus__RabbitMQ__HostName", "oroeventdrivenexchange")
    .WithEnvironment("EventBus__RabbitMQ__Port", "5672")
    .WithEnvironment("EventBus__RabbitMQ__UserName", "guest")
    .WithEnvironment("EventBus__RabbitMQ__Password", "guest")
    .WithEnvironment("IDENTITY_ADMIN_HTTP", "http://localhost:4200");

builder.Build().Run();
