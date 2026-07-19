using System.Globalization;
using IdentityServer.Client.Extensions;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.JSInterop;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddFluentUIComponents();
builder.Services.AddLocalization();

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthenticationStateDeserialization();

builder.Services.AddIdentityServerClientServices(new Uri(builder.HostEnvironment.BaseAddress));

var host = builder.Build();

// Match the culture the server already picked (from the ".AspNetCore.Culture" cookie set by
// /culture/set, or the browser's Accept-Language default) so WASM doesn't flash back to English
// after taking over from the server-rendered first paint.
var jsRuntime = host.Services.GetRequiredService<IJSRuntime>();
var culture = await jsRuntime.InvokeAsync<string?>("blazorCulture.get");
if (!string.IsNullOrEmpty(culture))
{
    var cultureInfo = new CultureInfo(culture);
    CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
    CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
}

await host.RunAsync();
