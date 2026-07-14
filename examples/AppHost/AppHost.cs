IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<RedisResource> redis = builder.AddRedis("redis");

IResourceBuilder<RabbitMQServerResource> rabbitMq = builder.AddRabbitMQ("oroeventdrivenexchange")
    .WithLifetime(ContainerLifetime.Persistent);

IResourceBuilder<PostgresServerResource> postgres = builder.AddPostgres("postgres")
    .WithPgAdmin()
    .WithDataVolume("oro-postgres-data");

IResourceBuilder<PostgresDatabaseResource> identityDb = postgres.AddDatabase("identitydb");

// Paramters
IResourceBuilder<ParameterResource> SymmetricSecurityKey = builder.AddParameter("SymmetricSecurityKey", "g9hLodrPUXAJRCxQUMZA6Bo2l8amqDjeHRerJIJAhVs=");
var clientId = builder.AddParameter("ClientId", "OroIdentityServer.Admin");

// --- Server Identity ---
IResourceBuilder<ProjectResource> identityServer = builder.AddProject<Projects.IdentityServer>("identity-api")
     .WithReference(rabbitMq).WaitFor(rabbitMq)
    .WithReference(identityDb).WaitFor(identityDb)
    .WithEnvironment("SymmetricSecurityKey", SymmetricSecurityKey);

// -- Frontend Identity Admin --
IResourceBuilder<NodeAppResource> identityAdmin = builder.AddPnpmApp(
    name: "identity-admin",
    workingDirectory: "../Frontends/oroidentity-admin");

identityAdmin.WithPnpmPackageInstallation()
    .WithReference(identityServer).WaitFor(identityServer)
    .WithHttpEndpoint(port: 30645, targetPort: 4200)
    .WithEnvironment("CLIENT_ID", clientId);

identityServer.WithReference(identityAdmin);

builder.Build().Run();
