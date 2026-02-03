using Microsoft.FluentUI.AspNetCore.Components;
using OroIdentity.Web.Server.Components;
using OroIdentity.Web.Server.Components.Pages.Account;
using OroIdentity.Web.Server.Extensiones;
using OroIdentity.Web.Server.Services;
using OroIdentity.Frontends.Services;
using OroBuildingBlocks.ServicesDefaults;
using Serilog;
using OroBuildingBlocks.Loggers;

var builder = WebApplication.CreateBuilder(args);

ConfigurationManager configuration = builder.Configuration;

Log.Logger = LoggerPrinter.CreateSerilogLogger("api", "OroIdentity.Web.Server", configuration);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddAuthenticationStateSerialization(
        options => options.SerializeAllClaims = true)
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();
builder.Services.AddFluentUIComponents();

builder.AddOroIdentityWebExtensions();
builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();

builder.Services.AddAuthenticationStateDeserialization();
builder.Services.AddDIOpenIddictApplication(configuration);

builder.Services.AddScoped<IdentityRedirectManager>();

builder.Services.AddAntiforgery();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();

var identityUri = configuration.GetSection("Identity:Url").Value;

builder.Services.AddScoped<ILoginService, LoginService>();
builder.Services.AddScoped<INavigationHistoryService, NavigationHistoryService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseWebAssemblyDebugging();
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.MapStaticAssets();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.MapIdentityEndpoints();

app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(OroIdentity.Web.Client._Imports).Assembly);

app.Run();
