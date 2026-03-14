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
        builder.ConfigureServices(services =>
        {
            // Remove existing OroIdentityAppContext registrations
            var dbContextDescriptor = services.SingleOrDefault(d => d.ServiceType != null && d.ServiceType.Name.Contains("DbContextOptions`1") && d.ServiceType.FullName!.Contains("OroIdentityAppContext"));
            if (dbContextDescriptor != null) services.Remove(dbContextDescriptor);

            // Create SQLite in-memory connection
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            services.AddDbContext<OroIdentityServer.Services.OroIdentityServer.Infraestructure.OroIdentityAppContext>(options =>
            {
                options.UseSqlite(_connection);
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
        return base.CreateHost(builder);
    }
}
