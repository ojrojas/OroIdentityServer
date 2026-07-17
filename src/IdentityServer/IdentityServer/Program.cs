using IdentityServer.Components;
using IdentityServer.Server.Extensions;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.FluentUI.AspNetCore.Components;
using OpenIddict.Abstractions;
using OroIdentityServer.Application.Extensions;
using OroIdentityServer.Infraestructure;
using OroIdentityServer.Infraestructure.Data;
using OroIdentityServer.Infraestructure.Repositories.Extensions;
using OroIdentityServer.Server.Authentication;
using OroIdentityServer.Server.Endpoints;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(AppContext.BaseDirectory, "data-protection-keys")))
    .SetApplicationName("OroIdentityServer");

// Add services to the container.
builder.Services.AddRazorComponents(options =>
{
    options.DetailedErrors = builder.Environment.IsDevelopment();
})    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents()
    .AddAuthenticationStateSerialization();

builder.Services.AddFluentUIComponents();
builder.Services.AddCascadingAuthenticationState();

builder.Services.AddControllers();


builder.AddInfraestructureExtensions(builder.Configuration);
builder.Services.AddApplicationHandlers();
builder.AddApplicationExtensions(builder.Configuration);


builder.AddIdentityServerOpenIddict();
builder.Services.AddAdminAuthentication();
builder.Services.AddAdminAuthorization();

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<OroIdentityServer.Core.Interfaces.IPasswordHasher, OroIdentityServer.Core.Services.PasswordHasher>();
builder.Services.AddScoped<AdminPasswordSignInService>();

var app = builder.Build();


await using (var scope = app.Services.CreateAsyncScope())
{
    var ctx = scope.ServiceProvider.GetRequiredService<OroIdentityAppContext>();
    await ctx.Database.MigrateAsync();

    var applicationManager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();
    var scopeManager = scope.ServiceProvider.GetRequiredService<IOpenIddictScopeManager>();
    var passwordHasher = scope.ServiceProvider.GetRequiredService<OroIdentityServer.Core.Interfaces.IPasswordHasher>();
    var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

    if (!configuration.GetValue<bool>("DatabaseSeeder:Skip"))
    {
        var seedPath = Path.Combine(AppContext.BaseDirectory, "Data", "seedData.json");
        if (File.Exists(seedPath))
        {
            await DatabaseSeeder.SeedAsync(ctx, applicationManager, seedPath, passwordHasher, configuration, scopeManager);
        }
    }
}


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

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapAuthEndpoints();
app.MapAdminApiEndpoints();
app.MapControllers();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(IdentityServer.Client._Imports).Assembly);

app.Run();

public partial class Program { }
