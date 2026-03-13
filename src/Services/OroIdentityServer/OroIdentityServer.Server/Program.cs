using Microsoft.AspNetCore.Components.Authorization;
using OroIdentityServer.Services.OroIdentityServer.Server.Components;
using OroIdentityServer.Services.OroIdentityServer.Server.Components.Account;
using Microsoft.FluentUI.AspNetCore.Components;
using OroIdentityServer.Services.OroIdentityServer.Core.Interfaces;
using OroBuildingBlocks.ServiceDefaults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

ConfigurationManager configuration = builder.Configuration;
Log.Logger = LoggerPrinter.CreateSerilogLogger("api", "OroIdentityServer", configuration);

// Shared DataProtection keys for cookie unprotect across apps
var keysFolder = Path.Combine(builder.Environment.ContentRootPath, "..", "..", "..", "..", "data-protection-keys");
Directory.CreateDirectory(keysFolder);
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(Path.GetFullPath(keysFolder)))
    .SetApplicationName("OroIdentityShared");

// Configure authentication cookie options
builder.Services.Configure<CookieAuthenticationOptions>(CookieAuthenticationDefaults.AuthenticationScheme, opts =>
{
    opts.Cookie.Name = "OroAuth";
    opts.Cookie.Path = "/";
    opts.Cookie.SameSite = SameSiteMode.Lax;
    opts.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

builder.Services.AddOpenApi();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddFluentUIComponents();

builder.Services.AddCascadingAuthenticationState();

builder.AddServicesWritersLogger(configuration);
builder.AddServiceDefaults();
builder.AddAppExtensions();
builder.AddApplicationExtensions(configuration);
builder.AddInfraestructureExtensions(configuration);
builder.AddOpenIddictExtensions(configuration);
builder.AddIdentityApiExtensions(configuration);
builder.AddCoreExtensions();

builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

builder.Services.Configure<RouteOptions>(options =>
{
    options.ConstraintMap["guid"] = typeof(GuidRouteConstraint);
});

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(
            builder.Configuration["IdentityWeb:Url"],
            builder.Configuration["IdentityAdmin:Url"]) 
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Enable in-memory cache and session support used by the server UI (transient status messages, etc.)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Path = "/";
    options.Cookie.SameSite = SameSiteMode.Lax;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
       app.MapOpenApi();
    app.MapScalarApiReference();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}


#if DEBUG
using var scope = app.Services.CreateScope();
var service = scope.ServiceProvider;

var context = service.GetRequiredService<OroIdentityAppContext>();
var applicationManager = service.GetRequiredService<IOpenIddictApplicationManager>();
var passwordHasher = service.GetRequiredService<IPasswordHasher>();

ArgumentNullException.ThrowIfNull(context);

var resetDb = configuration.GetValue<bool>("Debug:ResetDatabase", false);
if (resetDb)
{
    Console.WriteLine("Deleting database...");
    await context.Database.EnsureDeletedAsync();
}

Console.WriteLine("Applying pending migrations (if any)...");
await context.Database.MigrateAsync();
Console.WriteLine("Database migrated successfully.");
Console.WriteLine($"Database path: {context.Database.GetDbConnection().Database}");
Console.WriteLine($"Tables: {string.Join(", ", context.Model.GetEntityTypes().Select(t => t.GetTableName()))}");
var seedDataPath = Path.Combine(
    Directory.GetCurrentDirectory(),
    "bin", "Debug", "net10.0", "Data", "seedData.json");
// Log configured IdentityWeb URL so we can verify what the seeder will register
Console.WriteLine($"Configured IdentityWeb:Url = {configuration["IdentityWeb:Url"]}");
Console.WriteLine($"Configured Identity:Url = {configuration["Identity:Url"]}");
await DatabaseSeeder.SeedAsync(
    context,
    applicationManager,
    seedDataPath,
    passwordHasher,
    configuration);

#endif


app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseRouting();
app.UseCors();

// Ensure session middleware is available for components that rely on HttpContext.Session
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapDefaultEndpoints();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Endpoints
app.MapAuthorizeEndpoints()
.WithTags("AuthorizationEndpoints");

app.MapUsersQueriesEndpointsV1()
.WithTags("UserQueries");
app.MapUsersCommandsEndpointsV1()
.WithTags("UserCommands");

app.MapIdentificationTypeQueriesEndpointsV1()
.WithTags("IdentificationTypeQueries");
app.MapIdentificationTypeCommandsEndpointsV1()
.WithTags("IdentificationTypeCommands");

app.MapRoleQueriesEndpointsV1()
.WithTags("RoleQueries");
app.MapRoleCommandsEndpointsV1()
.WithTags("RoleCommands");

app.MapScopeQueriesEndpointsV1()
.WithTags("ScopeQueries");
app.MapScopeCommandsEndpointsV1()
.WithTags("ScopeCommands");

app.MapPermissionQueriesEndpointsV1()
.WithTags("PermissionQueries");
app.MapPermissionCommandsEndpointsV1()
.WithTags("PermissionCommands");

app.MapSessionQueriesEndpointsV1()
.WithTags("SessionQueries");
app.MapSessionCommandsEndpointsV1()
.WithTags("SessionCommands");

app.MapApplicationQueriesEndpointsV1()
.WithTags("ApplicationQueries");
app.MapApplicationCommandsEndpointsV1()
.WithTags("ApplicationCommands");

app.MapUserSessionQueriesEndpointsV1()
.WithTags("UserSessionQueries");
app.MapUserSessionCommandsEndpointsV1()
.WithTags("UserSessionCommands");

app.Run();
