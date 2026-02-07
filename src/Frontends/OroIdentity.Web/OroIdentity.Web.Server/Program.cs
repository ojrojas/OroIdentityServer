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
using OroIdentity.Web.Client.Constants;
using OroIdentity.Web.Server.Endpoints;

var builder = WebApplication.CreateBuilder(args);

ConfigurationManager configuration = builder.Configuration;

Log.Logger = LoggerPrinter.CreateSerilogLogger("api", "OroIdentity.Web.Server", configuration);

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

builder.Services.AddHttpContextAccessor();

builder.Services.AddHttpClient(
    OroIdentityWebConstants.OroIdentityServerApis, 
    client => { 
        client.BaseAddress = new Uri(builder.Configuration["Identity:Url"]) ?? 
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

app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(OroIdentity.Web.Client._Imports).Assembly);

app.Run();
