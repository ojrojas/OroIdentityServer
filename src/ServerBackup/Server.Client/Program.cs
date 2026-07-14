// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using OroIdentityServer.Server.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthenticationStateDeserialization();

builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
});

builder.Services.AddScoped<IUsersClient, UsersClient>();
builder.Services.AddScoped<IRolesClient, RolesClient>();
builder.Services.AddScoped<IPermissionsClient, PermissionsClient>();
builder.Services.AddScoped<ITenantsClient, TenantsClient>();
builder.Services.AddScoped<IIdentificationTypesClient, IdentificationTypesClient>();
builder.Services.AddScoped<IUserSessionsClient, UserSessionsClient>();
builder.Services.AddScoped<IApplicationsClient, ApplicationsClient>();
builder.Services.AddScoped<IScopesClient, ScopesClient>();

await builder.Build().RunAsync();
