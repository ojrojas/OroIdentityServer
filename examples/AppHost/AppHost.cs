using Microsoft.Extensions.Hosting;

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

// if (builder.Environment.IsDevelopment())
// {
//     IResourceBuilder<ContainerResource> identityServer = builder.AddContainer("identity-api", "localhost/oridentityserver", "latest")
//         // Aspire's https endpoint uses transport=http: the proxy terminates TLS and forwards
//         // plaintext HTTP to the container, so the app only needs plain HTTP listeners on 5080
//         // and 5086. This annotation makes the proxy use the development certificate.
//         .WithHttpsCertificateConfiguration(ctx =>
//         {
//             ctx.Arguments.Add("--https-certificate-path");
//             ctx.Arguments.Add(ctx.PfxPath);
//             ctx.EnvironmentVariables.Add("ASPNETCORE_Kestrel__Certificates__Default__Path", ctx.PfxPath);
//             ctx.EnvironmentVariables.Add("ASPNETCORE_Kestrel__Certificates__Default__Password", ctx.Password);
//             return Task.CompletedTask;
//         })
//         .WithHttpEndpoint(targetPort: 5080, port: 5080, name: "http")
//         .WithHttpsEndpoint(targetPort: 5086, port: 5086, name: "https")
//         .WithReference(rabbitMq).WaitFor(rabbitMq)
//         .WithReference(identityDb).WaitFor(identityDb)
//         .WithEnvironment("SEED_TENANT_NAME", "OroMasterTenant")
//         .WithEnvironment("ASPNETCORE_ENVIRONMENT", environment.EnvironmentName)
//         .WithEnvironment("SymmetricSecurityKey", SymmetricSecurityKey)
//         // .WithEnvironment("EventBus__RabbitMQ__HostName", "oroeventdrivenexchange")
//         // .WithEnvironment("EventBus__RabbitMQ__Port", "5672")
//         // .WithEnvironment("EventBus__RabbitMQ__UserName", "guest")
//         // .WithEnvironment("EventBus__RabbitMQ__Password", "guest")
//         .WithEnvironment("IDENTITY_ADMIN_HTTP", "http://localhost:4200")
//         ;

// }
// else
// {
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
// }


// example oroidentity-admin login angular client
var clientId = builder.AddParameter("ClientId", "OroIdentityServer.Admin");

var identityAdmin = builder.AddPnpmApp("oroidentity-admin", "../Frontends/oroidentity-admin").WithPnpmPackageInstallation();

identityAdmin.WithHttpEndpoint(port: 30645, targetPort: 4200)
   .WithEnvironment("CLIENT_ID", clientId)
   .WithEnvironment("IDENTITY_API_HTTPS", identityServer.GetEndpoint("https"))
   .WithEnvironment("IDENTITY_API_HTTP", identityServer.GetEndpoint("http"));

identityServer.WithReference(identityAdmin);

builder.Build().Run();
