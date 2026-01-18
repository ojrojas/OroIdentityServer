// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Client;
using OpenIddict.Abstractions;

namespace OroIdentity.Web.Server.Extensiones;

public static class OpenIddictExtensions
{
    public static IServiceCollection AddDIOpenIddictApplication(
        this IServiceCollection services, IConfiguration configuration)
    {
        var signingKey = configuration.GetSection("SymmetricSecurityKey").Value;

        ArgumentException.ThrowIfNullOrEmpty(signingKey, "SymmetricSecurityKey configuration is missing");

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
                    Convert.FromBase64String(signingKey)));

                // Register the System.Net.Http integration.
                config.UseSystemNetHttp();

                // Register the ASP.NET Core host.
                config.UseAspNetCore();
            }) 
            // .AddClient(options =>
            // {
            //     // Note: this sample uses the authorization code flow,
            //     // but you can enable the other flows if necessary.
            //     options.AllowAuthorizationCodeFlow()
            //            .AllowRefreshTokenFlow();

            //     // Register the signing and encryption credentials used to protect
            //     // sensitive data like the state tokens produced by OpenIddict.
            //     // Configure encryption and signing of tokens.  testing phrase tokens ORO_IDENTITY_SERVER_PROJECT_0001
            //     options.AddEncryptionKey(new SymmetricSecurityKey(
            //         Convert.FromBase64String(signingKey)));

            //     // Register the System.Net.Http integration and use the identity of the current
            //     // assembly as a more specific user agent, which can be useful when dealing with
            //     // providers that use the user agent as a way to throttle requests (e.g Reddit).
            //     options.UseSystemNetHttp()
            //            .SetProductInformation(typeof(Program).Assembly);
            // })
            ;

        return services;
    }
}