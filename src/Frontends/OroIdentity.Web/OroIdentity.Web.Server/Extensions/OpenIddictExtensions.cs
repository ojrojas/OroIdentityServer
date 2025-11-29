
using Microsoft.IdentityModel.Tokens;

namespace OroIdentity.Web.Server.Extensiones;

public static class OpenIddictExtensions
{
    public static IServiceCollection AddDIOpenIddictApplication(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOpenIddict()
            .AddValidation(config =>
            {
                config.SetIssuer($"{configuration["Identity:Url"]}/");
                config.AddAudiences("OroIdentityServer.Web");

                config.UseIntrospection()
                .SetClientId("OroIdentityServer.Web")
                .SetClientSecret("a2344152-e928-49e7-bb3c-ee54acc96c8c");

                // Configure encryption and signing of tokens.  testing phrase tokens ORO_IDENTITY_SERVER_PROJECT_0001
                config.AddEncryptionKey(new SymmetricSecurityKey(
                    Convert.FromBase64String(configuration.GetSection("SymmetricSecurityKey").Value)));

                // Register the System.Net.Http integration.
                config.UseSystemNetHttp();

                // Register the ASP.NET Core host.
                config.UseAspNetCore();
            });

        return services;
    }
}