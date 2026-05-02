var builder = DistributedApplication.CreateBuilder(args);
var postgres = builder.AddPostgres("postgres")
    .WithPgAdmin()
    .WithDataVolume("oro-postgres-data");

var identityDb = postgres.AddDatabase("identitydb", databaseName: "OroIdentity");

// --- Server Identity ---
var identityServer = builder.AddProject<Projects.OroIdentityServer_Server>("identity-api")
    .WithReference(identityDb)
    .WaitFor(identityDb);

// -- Frontend Identity Admin --
var identityAdmin = builder.AddPnpmApp(name: "identity-admin", workingDirectory: "../Frontends/identity-admin");
identityAdmin .WithPnpmPackageInstallation()
    .WithHttpEndpoint(30645, 4200);


identityServer.WithEnvironment("ACCOUNTANTS_WEB_HTTP", identityAdmin.GetEndpoint("http"));

builder.Build().Run();
