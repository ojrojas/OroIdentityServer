IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<RabbitMQServerResource> rabbitMq = builder.AddRabbitMQ("oroeventdrivenexchange")
    .WithLifetime(ContainerLifetime.Persistent);

IResourceBuilder<PostgresServerResource> postgres = builder.AddPostgres("postgres")
    .WithDataVolume("oro-postgres-data");

IResourceBuilder<PostgresDatabaseResource> identityDb = postgres.AddDatabase("identitydb");

IResourceBuilder<ParameterResource> SymmetricSecurityKey = builder.AddParameter("SymmetricSecurityKey", "g9hLodrPUXAJRCxQUMZA6Bo2l8amqDjeHRerJIJAhVs=");

// IResourceBuilder<ContainerResource> identityServer = builder.AddContainer("identity-api", "localhost/oridentityserver:latest")
//     .WithHttpEndpoint(targetPort: 5080, name: "http")
//     .WithReference(rabbitMq).WaitFor(rabbitMq)
//     .WithReference(identityDb).WaitFor(identityDb)
//     .WithEnvironment("SEED_TENANT_NAME", "OroMasterRealm")
//     .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
//     .WithEnvironment("SymmetricSecurityKey", SymmetricSecurityKey)
//     .WithEnvironment("EventBus__RabbitMQ__HostName", "oroeventdrivenexchange")
//     .WithEnvironment("EventBus__RabbitMQ__Port", "5672")
//     .WithEnvironment("EventBus__RabbitMQ__UserName", "guest")
//     .WithEnvironment("EventBus__RabbitMQ__Password", "guest")
//     .WithEnvironment("IDENTITY_ADMIN_HTTP", "http://localhost:4200");

    
IResourceBuilder<ProjectResource> identityServer = builder.AddProject<Projects.IdentityServer>("identity-api")
    .WithReference(rabbitMq).WaitFor(rabbitMq)
    .WithReference(identityDb).WaitFor(identityDb)
    .WithEnvironment("SEED_TENANT_NAME", "OroMasterRealm")
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
    .WithEnvironment("SymmetricSecurityKey", SymmetricSecurityKey)
    .WithEnvironment("EventBus__RabbitMQ__HostName", "oroeventdrivenexchange")
    .WithEnvironment("EventBus__RabbitMQ__Port", "5672")
    .WithEnvironment("EventBus__RabbitMQ__UserName", "guest")
    .WithEnvironment("EventBus__RabbitMQ__Password", "guest")
    .WithEnvironment("IDENTITY_ADMIN_HTTP", "http://localhost:4200");


builder.Build().Run();
