// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using OroIdentityServer.Infraestructure;
using OroIdentityServer.Infraestructure.Data;
using OroIdentityServer.Infraestructure.Repositories.Extensions;
using OroIdentityServer.Server;
using OroIdentityServer.Server.Authentication;
using OroIdentityServer.Server.Components;
using OroIdentityServer.Server.Endpoints;
using OroIdentityServer.Server.Extensions;
using OroIdentityServer.Server.OpenIddict;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents()
    .AddAuthenticationStateSerialization();

builder.Services.AddCascadingAuthenticationState();


Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(AppContext.BaseDirectory, "data-protection-keys")))
    .SetApplicationName("OroIdentityServer");

builder.AddInfraestructureExtensions(builder.Configuration);
builder.AddIdentityServerOpenIddict();
builder.Services.AddApplicationHandlers();
builder.Services.AddValidatorsFromAssembly(typeof(CreateTenantCommand).Assembly);
builder.Services.AddScoped<OroIdentityServer.Core.Interfaces.IPasswordHasher, OroIdentityServer.Core.Services.PasswordHasher>();
builder.Services.AddSingleton<OroBuildingBlocks.EventBus.Abstractions.IEventBus, OroIdentityServer.Server.Extensions.NoOpEventBus>();

builder.Services.AddAdminAuthentication();
builder.Services.AddAdminAuthorization();

builder.Services.AddAuthorization();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<AdminPasswordSignInService>();
builder.Services.AddSingleton<OroIdentityServer.Server.Components.Layout.ToastService>();

// Server-side client implementations (bypass HTTP for SSR)
builder.Services.AddScoped<OroIdentityServer.Server.Client.Services.IUsersClient, UsersServerClient>();
builder.Services.AddScoped<OroIdentityServer.Server.Client.Services.IRolesClient, RolesServerClient>();
builder.Services.AddScoped<OroIdentityServer.Server.Client.Services.IPermissionsClient, PermissionsServerClient>();
builder.Services.AddScoped<OroIdentityServer.Server.Client.Services.ITenantsClient, TenantsServerClient>();
builder.Services.AddScoped<OroIdentityServer.Server.Client.Services.IIdentificationTypesClient, IdentificationTypesServerClient>();
builder.Services.AddScoped<OroIdentityServer.Server.Client.Services.IUserSessionsClient, UserSessionsServerClient>();
builder.Services.AddScoped<OroIdentityServer.Server.Client.Services.IApplicationsClient, ApplicationsServerClient>();
builder.Services.AddScoped<OroIdentityServer.Server.Client.Services.IScopesClient, ScopesServerClient>();

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseWebAssemblyDebugging();
    app.UseMigrationsEndPoint();
    app.MapOpenApi();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.UseAuthentication();
app.UseAuthorization();

app.MapAuthEndpoints();
app.MapConnectEndpoints();
app.MapAdminApiEndpoints();


app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(OroIdentityServer.Server.Client._Imports).Assembly);

await using (var scope = app.Services.CreateAsyncScope())
{
    var ctx = scope.ServiceProvider.GetRequiredService<OroIdentityAppContext>();
    await ctx.Database.MigrateAsync();

    var applicationManager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();
    var scopeManager = scope.ServiceProvider.GetRequiredService<IOpenIddictScopeManager>();
    var passwordHasher = scope.ServiceProvider.GetRequiredService<OroIdentityServer.Core.Interfaces.IPasswordHasher>();
    var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

    var seedPath = Path.Combine(AppContext.BaseDirectory, "Data", "seedData.json");
    if (File.Exists(seedPath))
    {
        await DatabaseSeeder.SeedAsync(ctx, applicationManager, seedPath, passwordHasher, configuration, scopeManager);
    }
}

app.Run();

public partial class Program { }
