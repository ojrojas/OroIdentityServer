using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using OpenIddict.Validation.AspNetCore;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private SqliteConnection? _connection;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Provide minimal configuration values required by the tested app (e.g. symmetric signing key)
        builder.ConfigureAppConfiguration((ctx, conf) =>
        {
            var dict = new System.Collections.Generic.Dictionary<string, string?>
            {
                ["SymmetricSecurityKey"] = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("integration-test-signing-key-should-be-32bytes"))
            };
            conf.AddInMemoryCollection(dict);
        });

        builder.ConfigureServices(services =>
        {
            // Remove existing OroIdentityAppContext registrations
            var dbContextDescriptor = services.SingleOrDefault(d => d.ServiceType != null && d.ServiceType.Name.Contains("DbContextOptions`1") && d.ServiceType.FullName!.Contains("OroIdentityAppContext"));
            if (dbContextDescriptor != null) services.Remove(dbContextDescriptor);

            // Use EF Core InMemory provider for tests to avoid native SQLite/EF version issues
            services.AddDbContext<OroIdentityServer.Services.OroIdentityServer.Infraestructure.OroIdentityAppContext>(options =>
            {
                options.UseInMemoryDatabase("IntegrationTestsDb");
                options.UseOpenIddict();
            });

            // Add a test authentication scheme using the OpenIddict validation scheme name so endpoints requiring
            // OpenIddict validation will accept our test principal.
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
                options.DefaultScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
            })
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme, options => { });

            // Build the service provider to ensure DB is created before tests run
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<OroIdentityServer.Services.OroIdentityServer.Infraestructure.OroIdentityAppContext>();
            db.Database.EnsureCreated();
        });

        builder.UseEnvironment("IntegrationTest");
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        // Ensure configuration used by Program.Main (host-level) contains required symmetric key
        builder.ConfigureAppConfiguration((ctx, conf) =>
        {
            var dict = new System.Collections.Generic.Dictionary<string, string?>
            {
                ["SymmetricSecurityKey"] = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("integration-test-signing-key-should-be-32bytes"))
            };
            conf.AddInMemoryCollection(dict);
        });

        return base.CreateHost(builder);
    }
}
