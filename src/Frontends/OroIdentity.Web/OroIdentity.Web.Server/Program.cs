using Microsoft.FluentUI.AspNetCore.Components;
using OroIdentity.Web.Server.Components;
using OroIdentity.Web.Server.Components.Pages.Account;
using OroIdentity.Web.Server.Extensiones;
using OroIdentity.Web.Server.Services;

var builder = WebApplication.CreateBuilder(args);

ConfigurationManager configuration = builder.Configuration;

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddAuthenticationStateSerialization(
        options => options.SerializeAllClaims = true)
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();
builder.Services.AddFluentUIComponents();

builder.AddOroIdentityWebExtensions();
builder.Services.AddDIOpenIddictApplication(configuration);

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthenticationStateDeserialization();

builder.Services.AddScoped<IdentityRedirectManager>();

builder.Services.AddAntiforgery();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();

builder.Services.Configure<Microsoft.AspNetCore.Builder.SessionOptions>(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.Always;
});

var identityUri = configuration.GetSection("Identity:Url").Value;

builder.Services.AddHttpClient<LoginService>(configClient =>
{
    configClient.BaseAddress = new Uri(identityUri!);
    configClient.Timeout = TimeSpan.FromMinutes(2);
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
    {
        Console.WriteLine($"Certificate error: {errors}");
        return true; 
    }
});

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
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(OroIdentity.Web.Client._Imports).Assembly);

app.Run();
