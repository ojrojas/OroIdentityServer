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

var builder = WebApplication.CreateBuilder(args);

ConfigurationManager configuration = builder.Configuration;

Log.Logger = LoggerPrinter.CreateSerilogLogger("web.server", "OroIdentity.Web.Server", configuration);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents()
    .AddAuthenticationStateSerialization(
        options => options.SerializeAllClaims = true);
builder.Services.AddFluentUIComponents(options => options.ValidateClassNames = false);

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

builder.AddServiceDefaults();

builder.Services.AddHttpClient<IApplicationsService, ApplicationsService>(
    client => { 
        client.BaseAddress = new Uri(identityUri) ?? 
        throw new Exception("Missing base address environment");
    }
).AddHttpMessageHandler<TokenHandler>();

builder.Services.AddHttpClient<IRolesService, RolesService>(
    client => { 
        client.BaseAddress = new Uri(identityUri) ?? 
        throw new Exception("Missing base address environment");
    }
).AddHttpMessageHandler<TokenHandler>();

builder.Services.AddHttpClient<IScopesService, ScopesService>(
    client => { 
        client.BaseAddress = new Uri(identityUri) ?? 
        throw new Exception("Missing base address environment");
    }
).AddHttpMessageHandler<TokenHandler>();

builder.Services.AddHttpClient<IUsersService, UsersService>(
    client => { 
        client.BaseAddress = new Uri(identityUri) ?? 
        throw new Exception("Missing base address environment");
    }
).AddHttpMessageHandler<TokenHandler>();

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
app.MapScopesEndpointsV1().RequireAuthorization();
app.MapUsersEndpointsV1().RequireAuthorization();

app.MapDefaultEndpoints();

app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(OroIdentity.Web.Client._Imports).Assembly);

app.Run();
