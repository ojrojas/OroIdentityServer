// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Services.OroIdentityServer.Server.Extensions;

public static class OpenIddictExtensions
{
    public static void AddOpenIddictExtensions(this IHostApplicationBuilder builder, IConfiguration configuration)
    {
        builder.Services.AddOpenIddict()
        .AddCore(config =>
        {
            config.UseEntityFrameworkCore()
            .UseDbContext<OroIdentityAppContext>();

            config.UseQuartz();
        })

        .AddServer(config =>
        {
            // Enable the authorization, logout, token and userinfo endpoints.
            config.SetAuthorizationEndpointUris("/connect/authorize")
                  .SetEndSessionEndpointUris("/connect/logout")
                  .SetTokenEndpointUris("/connect/token")
                  .SetIntrospectionEndpointUris("/connect/introspect")
                  // .SetUserInfoEndpointUris("/connect/userinfo")
                  ;

            // Note: the sample uses the code and refresh token flows but you can enable
            // the other flows if you need to support implicit, password or client credentials.
            config.AllowAuthorizationCodeFlow()
                       .AllowHybridFlow()
                       .AllowImplicitFlow()
                       .AllowNoneFlow()
                       .AllowPasswordFlow()
                       .AllowRefreshTokenFlow()
                       .AllowTokenExchangeFlow();

            config.RequireProofKeyForCodeExchange();

            // Mark the "email", "profile" and "roles" scopes as supported scopes.
            config.RegisterScopes(Scopes.Email, Scopes.Profile, Scopes.Roles);

            var signingKey = configuration.GetSection("SymmetricSecurityKey").Value;

            ArgumentException.ThrowIfNullOrEmpty(signingKey, "SymmetricSecurityKey configuration is missing");

            // Configure encryption and signing of tokens.  testing phrase tokens ORO_IDENTITY_SERVER_PROJECT_0001
            config.AddEncryptionKey(new SymmetricSecurityKey(                                         
                Convert.FromBase64String(signingKey)));
    
         


            config.SetAccessTokenLifetime(TimeSpan.FromHours(1));
            config.SetRefreshTokenLifetime(TimeSpan.FromDays(30));

            // Register the signing and encryption credentials.
            config.AddDevelopmentEncryptionCertificate()
                   .AddDevelopmentSigningCertificate();

            // Register the ASP.NET Core host and configure the ASP.NET Core-specific config.
            config.UseAspNetCore()
                   .DisableTransportSecurityRequirement()
                   .EnableAuthorizationEndpointPassthrough()
                   .EnableEndSessionEndpointPassthrough()
                   .EnableStatusCodePagesIntegration()
                   // .EnableUserInfoEndpointPassthrough()
                   .EnableTokenEndpointPassthrough();
        })

        .AddValidation(config =>
        {
            // Import the configuration from the local OpenIddict server instance.
            config.UseLocalServer();

            // Register the ASP.NET Core host.
            config.UseAspNetCore();

            config.EnableAuthorizationEntryValidation();
        })
        ;
    }
}