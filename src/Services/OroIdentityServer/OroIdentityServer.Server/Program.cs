using Microsoft.AspNetCore.Components.Authorization;
using OroIdentityServer.Services.OroIdentityServer.Server.Components;
using OroIdentityServer.Services.OroIdentityServer.Server.Components.Account;
using Microsoft.FluentUI.AspNetCore.Components;
using OroIdentityServer.Services.OroIdentityServer.Core.Interfaces;

var builder = WebApplication.CreateBuilder(args);

IConfiguration configuration = builder.Configuration;
Log.Logger = LoggerPrinter.CreateSerilogLogger("api", "OroIdentityServer", configuration);
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
Console.WriteLine("Deleting database...");
await context.Database.EnsureDeletedAsync();
Console.WriteLine("Creating database...");
await context.Database.EnsureCreatedAsync();
Console.WriteLine("Database created successfully.");
Console.WriteLine($"Database path: {context.Database.GetDbConnection().Database}");
Console.WriteLine($"Tables: {string.Join(", ", context.Model.GetEntityTypes().Select(t => t.GetTableName()))}");
var seedDataPath = Path.Combine(
    Directory.GetCurrentDirectory(),
    "bin", "Debug", "net10.0", "Data", "seedData.json");
await DatabaseSeeder.SeedAsync(
    context, 
    applicationManager, 
    seedDataPath, 
    passwordHasher,
    configuration);

#endif


app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseRouting();
app.UseAuthorization();

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

app.MapApplicationQueriesEndpointsV1()
.WithTags("ApplicationQueries");
app.MapApplicationCommandsEndpointsV1()
.WithTags("ApplicationCommands");

app.Run();
