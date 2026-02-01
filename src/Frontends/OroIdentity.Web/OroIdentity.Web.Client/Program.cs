using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.FluentUI.AspNetCore.Components;
using OroIdentity.Frontends.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddFluentUIComponents();
builder.Services.AddScoped<INavigationHistoryService, NavigationHistoryService>();


await builder.Build().RunAsync();
