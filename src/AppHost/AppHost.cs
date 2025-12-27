// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);

var configuration = builder.Configuration;

var launchProfile = ShouldUseHttpForEndpoints(configuration) ? Constants.Http : Constants.Https;

var seq = builder.AddSeq(Constants.Seq);
var postgres = builder.AddPostgres("postgres")
.WithImage("ankane/pgvector")
    .WithImageTag("latest").WithPgAdmin(config =>
    {
        config.WithImageTag("latest");
    });

var identityDb = postgres.AddDatabase("identitydb");

var identity = builder.AddProject<Projects.OroIdentityServer_Api>(Constants.IdentityApi, launchProfile);
var identityWeb = builder.AddProject<Projects.OroIdentity_Web_Server>(Constants.IdentityWeb, launchProfile);

identity
.WaitFor(identityDb)
.WithReference(seq)
.WithReference(identityDb)
.WithReference(identityWeb)
.WithEnvironment("IdentityWeb__Url", identityWeb.GetEndpoint(launchProfile))
.WithExternalHttpEndpoints();

identityWeb
.WaitFor(identity)
.WithReference(seq)
.WithReference(identity)
.WithEnvironment("Identity__Url", identity.GetEndpoint(launchProfile))
.WithExternalHttpEndpoints();

builder.Build().Run();


static bool ShouldUseHttpForEndpoints(IConfiguration configuration)
{
    const string EnvVarName = "OROIDENTITYSERVER_USE_HTTP_ENDPOINTS";
    var envValue = configuration.GetSection(EnvVarName).Value;

    // Attempt to parse the environment variable value; return true if it's exactly "1".
    return int.TryParse(envValue, out int result) && result == 1;
}