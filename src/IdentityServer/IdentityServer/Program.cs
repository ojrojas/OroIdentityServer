using IdentityServer.Client.Extensions;
using IdentityServer.Client.Services;
using System.Globalization;
using IdentityServer.Components;
using IdentityServer.Server.Extensions;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
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

// builder.Services.AddDataProtection()
//     .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(AppContext.BaseDirectory, "data-protection-keys")))
//     .SetApplicationName("OroIdentityServer");

// Add services to the container.
builder.Services.AddRazorComponents(options =>
{
    options.DetailedErrors = builder.Environment.IsDevelopment();
}).AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents()
    .AddAuthenticationStateSerialization();

builder.Services.AddIdentityServerUiServices();
builder.Services.AddCascadingAuthenticationState();

builder.Services.AddLocalization();

builder.Services.AddControllersWithViews();

builder.AddInfraestructureExtensions(builder.Configuration);
builder.Services.AddApplicationHandlers();
builder.AddApplicationExtensions(builder.Configuration);
builder.Services.AddServerAdminServices();


builder.AddIdentityServerOpenIddict();
builder.Services.AddAdminAuthentication();
builder.Services.AddAdminAuthorization();

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<OroIdentityServer.Core.Interfaces.IPasswordHasher, OroIdentityServer.Core.Services.PasswordHasher>();
builder.Services.AddScoped<AdminPasswordSignInService>();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services.AddCors(setupAction =>
{
    setupAction.AddPolicy("OroIdentityServer", policy =>
    {
        policy.AllowAnyHeader()
        .AllowAnyMethod()
        .AllowAnyOrigin();
    });
});

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
            await DatabaseSeeder.SeedAsync(ctx, passwordHasher, configuration);
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

app.UseCors("OroIdentityServer");

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseRequestLocalization();

app.UseAuthentication();
app.UseAuthorization();

// Resolve the current tenant from the header the WASM client sends on every /api call, or from the
// oro_tenant cookie persisted by the tenant switcher. Applied before rendering so pages and queries
// are already scoped to the selected tenant on the first pass.
app.Use(async (context, next) =>
{
    Guid? tenantId = null;

    if (context.Request.Headers.TryGetValue("X-Tenant-Id", out var tenantHeader)
        && Guid.TryParse(tenantHeader.ToString(), out var headerTenantId))
    {
        tenantId = headerTenantId;
    }
    else if (context.Request.Cookies.TryGetValue("oro_tenant", out var cookieTenant)
             && Guid.TryParse(cookieTenant, out var cookieTenantId))
    {
        tenantId = cookieTenantId;
    }

    // The is_master_admin claim is added by AdminPasswordSignInService and survives cookie
    // round-trips, so the data layer can scope queries to the current user's accessible tenants
    // without re-querying TenantUser memberships on every request.
    var isMasterAdmin = context.User?.Identity?.IsAuthenticated == true
        && context.User.HasClaim(OroIdentityServer.Server.Authentication.AdminPasswordSignInService.IsMasterAdminClaimType, "true");

    var tenantContext = context.RequestServices.GetRequiredService<ICurrentTenantContext>();
    if (tenantId is { } id)
        tenantContext.SetCurrentTenantId(id);
    tenantContext.SetMasterAdmin(isMasterAdmin);

    await next();
});

app.Use(async (context, next) =>
{
    var path = context.Request.Path;
    var isExempt = path.StartsWithSegments("/Account")
        || path.StartsWithSegments("/auth")
        || path.StartsWithSegments("/connect")
        || path.StartsWithSegments("/api")
        || path.StartsWithSegments("/_blazor")
        || path.StartsWithSegments("/_framework")
        || path.StartsWithSegments("/css")
        || path.StartsWithSegments("/js")
        || path.StartsWithSegments("/culture");

    if (!isExempt
        && HttpMethods.IsGet(context.Request.Method)
        && context.User.Identity?.IsAuthenticated == true
        && context.User.HasClaim(c => c.Type == AdminPasswordSignInService.MustChangePasswordClaimType)
        // Only lock down page navigations, not static asset requests (CSS/JS/WASM), otherwise
        // app.css and the Blazor framework modules get 302-redirected and served as HTML.
        && context.Request.Headers.Accept.Any(a => a is not null && a.Contains("text/html", StringComparison.OrdinalIgnoreCase)))
    {
        context.Response.Redirect("/Account/ChangePassword");
        return;
    }

    await next();
});

app.UseAntiforgery();
var supportedCultures = new[]
{
    new CultureInfo("en"),
    new CultureInfo("de"),
    new CultureInfo("fr"),
    new CultureInfo("it"),
    new CultureInfo("es"),
    new CultureInfo("ja"),
    new CultureInfo("br"),
    new CultureInfo("zh")
};

app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("en"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures,
    RequestCultureProviders =
    [
        new CookieRequestCultureProvider(),
        new AcceptLanguageHeaderRequestCultureProvider()
    ]
});


app.MapGet("/culture/set", (HttpContext http, string culture, string? redirectUri) =>
{
    http.Response.Cookies.Append(
        CookieRequestCultureProvider.DefaultCookieName,
        CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
        new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1), IsEssential = true });

    return Results.Redirect(string.IsNullOrEmpty(redirectUri) ? "/" : redirectUri);
});

app.MapAuthEndpoints();
app.MapAdminApiEndpoints();
app.MapControllers();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(IdentityServer.Client._Imports).Assembly);

app.Run();


