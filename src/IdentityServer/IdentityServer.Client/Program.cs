using IdentityServer.Client.Extensions;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddLocalization();

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthenticationStateDeserialization();

builder.Services.AddIdentityServerClientServices(new Uri(builder.HostEnvironment.BaseAddress));
builder.Services.AddIdentityServerUiServices();

var host = builder.Build();

await host.RunAsync();
