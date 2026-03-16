using Microsoft.FluentUI.AspNetCore.Components;
using OroIdentity.Web.Server.Components;
using OroIdentity.Web.Server.Components.Pages.Account;
using OroIdentity.Web.Server.Extensiones;
using OroIdentity.Web.Server.Services;
using OroIdentity.Frontends.Services;
using OroBuildingBlocks.ServicesDefaults;
using Serilog;
using OroBuildingBlocks.Loggers;
using OroIdentity.Web.Client.Interfaces;
using OroIdentity.Web.Server.Handlers;
using OroIdentity.Web.Server.Endpoints;
using OroBuildingBlocks.ServiceDefaults;
using OroIdentity.Web.Client.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

ConfigurationManager configuration = builder.Configuration;

Log.Logger = LoggerPrinter.CreateSerilogLogger("web.server", "OroIdentity.Web.Server", configuration);

// Shared DataProtection keys for cookie unprotect across apps
var keysFolder = Path.Combine(builder.Environment.ContentRootPath, "..", "..", "..", "..", "data-protection-keys");
Directory.CreateDirectory(keysFolder);
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(Path.GetFullPath(keysFolder)))
    .SetApplicationName("OroIdentityShared");

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents()
    .AddAuthenticationStateSerialization(
        options => options.SerializeAllClaims = true);
builder.Services.AddFluentUIComponents(options => options.ValidateClassNames = false);

builder.AddAppWebExtensions();

builder.AddOroIdentityWebExtensions();

builder.Services.AddDIOpenIddictApplication(configuration);

builder.Services.AddScoped<IdentityRedirectManager>();

builder.Services.AddAntiforgery();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();
builder.Services.AddScoped<TokenHandler>();

var identityUri = configuration.GetSection("Identity:Url").Value;

builder.Services.AddScoped<ILoginService, LoginService>();
builder.Services.AddScoped<INavigationHistoryService, NavigationHistoryService>();
builder.Services.AddScoped<IApplicationsService, ApplicationsService>();

builder.Services.AddScoped<IThemeService, ThemeService>();

builder.Services.AddHttpContextAccessor();

// When initiating the OAuth/OpenId Connect challenge, set a flag that indicates whether the client
// app expects a local sign-in to be performed by the callback handler. This prevents the ServiceDefaults
// callback from creating cookies unless explicitly requested by the initiator.
// Usage: when calling /account/login, set authentication properties Parameters["local_signin"] = "true" if a local cookie is desired.

builder.AddServiceDefaults();

builder.AddAppHttpClientExtensions(identityUri);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

await using var scope = app.Services.CreateAsyncScope();
var context = scope.ServiceProvider.GetRequiredService<DbContext>();
await context.Database.EnsureCreatedAsync();

app.MapStaticAssets();

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.MapIdentityEndpoints();
app.MapApplicationEndpointsV1().RequireAuthorization();
app.MapRolesEndpointsV1().RequireAuthorization();
app.MapTenantsEndpointsV1().RequireAuthorization();
app.MapRoleClaimsEndpointsV1().RequireAuthorization();
app.MapPermissionsEndpointsV1().RequireAuthorization();
app.MapScopesEndpointsV1().RequireAuthorization();
app.MapUsersEndpointsV1().RequireAuthorization();
app.MapIdentificationTypesEndpointsV1().RequireAuthorization();
app.MapSessionsEndpointsV1().RequireAuthorization();

app.MapDefaultEndpoints();

app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(OroIdentity.Web.Client._Imports).Assembly);

app.Run();
