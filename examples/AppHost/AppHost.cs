IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

var environment = builder.Environment;

IResourceBuilder<RabbitMQServerResource> rabbitMq = builder.AddRabbitMQ("oroeventdrivenexchange")
    .WithLifetime(ContainerLifetime.Persistent);

IResourceBuilder<PostgresServerResource> postgres = builder.AddPostgres("postgres")
    .WithDataVolume("oro-postgres-data");

postgres.WithPgAdmin(container => container.WithImageTag("latest"));

IResourceBuilder<PostgresDatabaseResource> identityDb = postgres.AddDatabase("identitydb");

IResourceBuilder<ParameterResource> SymmetricSecurityKey = builder.AddParameter("SymmetricSecurityKey", "g9hLodrPUXAJRCxQUMZA6Bo2l8amqDjeHRerJIJAhVs=");

IResourceBuilder<ContainerResource> identityServer = builder.AddContainer("identity-api", "localhost/oridentityserver", "latest")
    // Aspire's https endpoint uses transport=http: the proxy terminates TLS and forwards
    // plaintext HTTP to the container, so the app only needs plain HTTP listeners on 5080
    // and 5086. This annotation makes the proxy use the development certificate.
    .WithHttpsCertificateConfiguration(ctx =>
    {
        ctx.Arguments.Add("--https-certificate-path");
        ctx.Arguments.Add(ctx.PfxPath);
        ctx.EnvironmentVariables.Add("ASPNETCORE_Kestrel__Certificates__Default__Path", ctx.PfxPath);
        ctx.EnvironmentVariables.Add("ASPNETCORE_Kestrel__Certificates__Default__Password", ctx.Password);
        return Task.CompletedTask;}
    )
    .WithHttpEndpoint(targetPort: 5080, port:5080, name: "http")
    .WithHttpsEndpoint(targetPort: 5086, port: 5086, name: "https")
    .WithReference(rabbitMq).WaitFor(rabbitMq)
    .WithReference(identityDb).WaitFor(identityDb)
    .WithEnvironment("SEED_TENANT_NAME", "OroMasterTenant")
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", environment.EnvironmentName)
    .WithEnvironment("SymmetricSecurityKey", SymmetricSecurityKey)
    .WithEnvironment("EventBus__RabbitMQ__HostName", "oroeventdrivenexchange")
    .WithEnvironment("EventBus__RabbitMQ__Port", "5672")
    .WithEnvironment("EventBus__RabbitMQ__UserName", "guest")
    .WithEnvironment("EventBus__RabbitMQ__Password", "guest")
    .WithEnvironment("IDENTITY_ADMIN_HTTP", "http://localhost:4200")
    ;

    
// IResourceBuilder<ProjectResource> identityServer = builder.AddProject<Projects.IdentityServer>("identity-api")
//     .WithReference(rabbitMq).WaitFor(rabbitMq)
//     .WithReference(identityDb).WaitFor(identityDb)
//     .WithEnvironment("SEED_TENANT_NAME", "OroMasterTenant")
//     .WithEnvironment("SEED_ADMIN_ROLE", "Administrator")
//     .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
//     .WithEnvironment("SymmetricSecurityKey", SymmetricSecurityKey)
//     .WithEnvironment("EventBus__RabbitMQ__HostName", "oroeventdrivenexchange")
//     .WithEnvironment("EventBus__RabbitMQ__Port", "5672")
//     .WithEnvironment("EventBus__RabbitMQ__UserName", "guest")
//     .WithEnvironment("EventBus__RabbitMQ__Password", "guest")
//     .WithEnvironment("IDENTITY_ADMIN_HTTP", "http://localhost:4200");


builder.Build().Run();
