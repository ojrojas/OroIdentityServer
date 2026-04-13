using Microsoft.AspNetCore.Components.Authorization;
using OroIdentityServer.Server.Components;
using OroIdentityServer.Server.Components.Account;
using Microsoft.FluentUI.AspNetCore.Components;
using OroIdentityServer.Core.Interfaces;
using OroBuildingBlocks.ServiceDefaults;
using Microsoft.AspNetCore.DataProtection;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;

var builder = WebApplication.CreateBuilder(args);

ConfigurationManager configuration = builder.Configuration;
Log.Logger = LoggerPrinter.CreateSerilogLogger("api", "OroIdentityServer", configuration);

// Shared DataProtection keys for cookie unprotect across apps
var keysFolder = Path.Combine(builder.Environment.ContentRootPath, "..", "..", "..", "data-protection-keys");
Directory.CreateDirectory(keysFolder);
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(Path.GetFullPath(keysFolder)))
    .SetApplicationName("OroIdentityShared");

// Configure authentication cookie options
builder.Services.Configure<CookieAuthenticationOptions>(CookieAuthenticationDefaults.AuthenticationScheme, opts =>
{
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

var listOfOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(listOfOrigins)
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

// Server port discovery: register provider and initializer hosted service
builder.Services.AddSingleton<ServerPortProvider>();
builder.Services.AddHostedService<ServerPortInitializerHostedService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
    app.MapOpenApi();
    app.MapScalarApiReference();

    using var scope = app.Services.CreateScope();
    var service = scope.ServiceProvider;

    var context = service.GetRequiredService<OroIdentityAppContext>();
    var applicationManager = service.GetRequiredService<IOpenIddictApplicationManager>();
    var passwordHasher = service.GetRequiredService<IPasswordHasher>();
    var scopeManager = service.GetRequiredService<IOpenIddictScopeManager>();

    ArgumentNullException.ThrowIfNull(context);

    //await context.Database.EnsureDeletedAsync();
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
        configuration,
        scopeManager);
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

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

app.MapTenantEndpoints();
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

// Helper: stores discovered server ports and exposes readiness
public sealed class ServerPortProvider
{
    private readonly List<int> _ports = new();
    private readonly TaskCompletionSource<bool> _ready = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public IReadOnlyList<int> Ports => _ports.AsReadOnly();
    public bool IsReady => _ready.Task.IsCompleted;
    public Task WaitForReadyAsync() => _ready.Task;

    internal void SetPorts(IEnumerable<int> ports)
    {
        _ports.Clear();
        _ports.AddRange(ports);
        _ready.TrySetResult(true);
    }
}

// Hosted service: runs on startup, inspects the server (features or via reflection) and fills the provider
public sealed class ServerPortInitializerHostedService : Microsoft.Extensions.Hosting.IHostedService
{
    private readonly IServiceProvider _services;
    private readonly Microsoft.Extensions.Logging.ILogger<ServerPortInitializerHostedService> _logger;

    public ServerPortInitializerHostedService(IServiceProvider services, Microsoft.Extensions.Logging.ILogger<ServerPortInitializerHostedService> logger)
    {
        _services = services;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            // ensure we don't block startup sync contexts
            await Task.Yield();

            using var scope = _services.CreateScope();
            var provider = scope.ServiceProvider;
            var ports = new List<int>();

            // Try to get IServer and then IServerAddressesFeature
            var server = provider.GetService(typeof(Microsoft.AspNetCore.Hosting.Server.IServer)) as Microsoft.AspNetCore.Hosting.Server.IServer;
            if (server != null)
            {
                var featuresProp = server.GetType().GetProperty("Features", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                var features = featuresProp?.GetValue(server) as Microsoft.AspNetCore.Http.Features.IFeatureCollection;
                var addressesFeature = features?.Get<Microsoft.AspNetCore.Hosting.Server.Features.IServerAddressesFeature>();
                if (addressesFeature != null)
                {
                    foreach (var address in addressesFeature.Addresses)
                    {
                        if (Uri.TryCreate(address, UriKind.Absolute, out var uri) && uri.Port > 0)
                            ports.Add(uri.Port);
                        else
                        {
                            var idx = address.LastIndexOf(':');
                            if (idx > -1 && int.TryParse(address.Substring(idx + 1), out var p))
                                ports.Add(p);
                        }
                    }
                }

                // Reflection fallback into Kestrel internals
                if (!ports.Any() && server.GetType().FullName?.Contains("KestrelServer") == true)
                {
                    object kestrelOptions = null;
                    var optionsField = server.GetType().GetField("_options", BindingFlags.Instance | BindingFlags.NonPublic);
                    if (optionsField != null)
                        kestrelOptions = optionsField.GetValue(server);
                    else
                    {
                        var optProp = server.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                            .FirstOrDefault(p => p.PropertyType.Name.Contains("KestrelServerOptions"));
                        if (optProp != null)
                            kestrelOptions = optProp.GetValue(server);
                    }

                    if (kestrelOptions != null)
                    {
                        var listenProp = kestrelOptions.GetType().GetProperty("ListenOptions", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                        var listenOpts = listenProp?.GetValue(kestrelOptions) as IEnumerable;
                        if (listenOpts != null)
                        {
                            foreach (var lo in listenOpts)
                            {
                                var epProp = lo.GetType().GetProperty("EndPoint", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                                var ep = epProp?.GetValue(lo) as System.Net.EndPoint;
                                if (ep is System.Net.IPEndPoint ipe)
                                {
                                    ports.Add(ipe.Port);
                                }
                                else
                                {
                                    var portProp = lo.GetType().GetProperty("Port", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                                    if (portProp != null && portProp.GetValue(lo) is int p)
                                        ports.Add(p);
                                }
                            }
                        }
                    }
                }
            }

            // Config fallback (ASPNETCORE_URLS / urls)
            if (!ports.Any())
            {
                var config = provider.GetService(typeof(Microsoft.Extensions.Configuration.IConfiguration)) as Microsoft.Extensions.Configuration.IConfiguration;
                var urls = config? ["ASPNETCORE_URLS"] ?? config?["urls"] ?? config?["Urls"];
                if (!string.IsNullOrEmpty(urls))
                {
                    foreach (var part in urls.Split(';', StringSplitOptions.RemoveEmptyEntries))
                    {
                        if (Uri.TryCreate(part, UriKind.Absolute, out var uri2) && uri2.Port > 0)
                            ports.Add(uri2.Port);
                        else
                        {
                            var idx = part.LastIndexOf(':');
                            if (idx > -1 && int.TryParse(part.Substring(idx + 1), out var p))
                                ports.Add(p);
                        }
                    }
                }
            }

            var providerSingleton = provider.GetService(typeof(ServerPortProvider)) as ServerPortProvider;
            providerSingleton?.SetPorts(ports.Distinct().ToList());
            _logger.LogInformation("Server ports discovered: {ports}", string.Join(", ", ports.Distinct()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not determine server ports via reflection.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
