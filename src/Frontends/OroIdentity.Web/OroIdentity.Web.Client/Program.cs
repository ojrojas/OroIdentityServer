// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.FluentUI.AspNetCore.Components;
using OroIdentity.Frontends.Services;
using OroIdentity.Web.Client.Interfaces;
using OroIdentity.Web.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthenticationStateDeserialization();

builder.Services.AddScoped<INavigationHistoryService, NavigationHistoryService>();


builder.Services.AddHttpClient<IApplicationsService, ApplicationsClientService>(
    client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
);

builder.Services.AddHttpClient<IRolesService, RolesClientService>(
    client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
);

builder.Services.AddHttpClient<IScopesService, ScopesClientService>(
    client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
);

builder.Services.AddHttpClient<IUsersService, UsersClientService>(
    client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
);


builder.Services.AddFluentUIComponents();


await builder.Build().RunAsync();
