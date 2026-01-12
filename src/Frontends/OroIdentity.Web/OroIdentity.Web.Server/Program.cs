using Microsoft.FluentUI.AspNetCore.Components;
using OroIdentity.Web.Server.Components;
using OroIdentity.Web.Server.Components.Pages.Account;
using OroIdentity.Web.Server.Extensiones;
using OroIdentity.Web.Server.Services;

var builder = WebApplication.CreateBuilder(args);

IConfiguration configuration = builder.Configuration;

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddAuthenticationStateSerialization(
        options => options.SerializeAllClaims = true)
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();
builder.Services.AddFluentUIComponents();

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthenticationStateDeserialization();

builder.Services.AddScoped<IdentityRedirectManager>();

builder.AddOroIdentityWebExtensions();
builder.Services.AddDIOpenIddictApplication(configuration);

builder.Services.AddAntiforgery();


var identityUri = configuration.GetSection("Identity:Url").Value;

builder.Services.AddHttpClient<LoginService>(configClient =>
{
    configClient.BaseAddress = new Uri(identityUri!);
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
app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(OroIdentity.Web.Client._Imports).Assembly);

app.Run();
