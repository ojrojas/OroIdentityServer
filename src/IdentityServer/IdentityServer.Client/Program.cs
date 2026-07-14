using IdentityServer.Client.Interfaces;
using IdentityServer.Client.Services;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.FluentUI.AspNetCore.Components;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddFluentUIComponents();

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthenticationStateDeserialization();


builder.Services.AddHttpClient<IAdminService, AdminService>(
    httpClient =>
        httpClient.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
);


await builder.Build().RunAsync();
