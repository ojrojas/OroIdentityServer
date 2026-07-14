// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using Microsoft.IdentityModel.Tokens;
using OroIdentityServer.Infraestructure;
using Quartz;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace OroIdentityServer.Server.OpenIddict;

public static class OpenIddictServerConfiguration
{
    public static TBuilder AddIdentityServerOpenIddict<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.Services.AddQuartz(options =>
        {
            options.UseSimpleTypeLoader();
            options.UseInMemoryStore();
        });

        builder.Services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

        builder.Services.AddOpenIddict()
            .AddCore(options =>
            {
                options.UseEntityFrameworkCore()
                    .UseDbContext<OroIdentityAppContext>();

                options.UseQuartz();
            })
            .AddServer(options =>
            {
                options.SetAuthorizationEndpointUris("connect/authorize")
                    .SetEndSessionEndpointUris("connect/logout")
                    .SetTokenEndpointUris("connect/token")
                    .SetUserInfoEndpointUris("connect/userinfo")
                    .SetIntrospectionEndpointUris("connect/introspect");

                options.AllowAuthorizationCodeFlow()
                    .AllowRefreshTokenFlow()
                    .AllowClientCredentialsFlow()
                    .RequireProofKeyForCodeExchange();

                options.RegisterScopes(
                    Scopes.OpenId,
                    Scopes.Profile,
                    Scopes.Email,
                    Scopes.Roles,
                    Scopes.OfflineAccess,
                    "admin");

                var symmetricSecurityKey = new SymmetricSecurityKey(
                    // itentional_propose_migrations -> to migration, no real value
                Convert.FromBase64String(builder.Configuration.GetValue<string>("SymmetricSecurityKey") ?? "aXRlbnRpb25hbF9wcm9wb3NlX21pZ3JhdGlvbnM="));

                if (builder.Environment.IsDevelopment())
                    options.AddDevelopmentEncryptionCertificate()
                        .AddDevelopmentSigningCertificate();
                else options.AddEncryptionKey(symmetricSecurityKey)
                    .AddSigningKey(symmetricSecurityKey);

                options.UseAspNetCore()
                    .EnableAuthorizationEndpointPassthrough()
                    .EnableEndSessionEndpointPassthrough()
                    .EnableTokenEndpointPassthrough()
                    .EnableUserInfoEndpointPassthrough()
                    .EnableStatusCodePagesIntegration()
                    .DisableTransportSecurityRequirement();
            })
            .AddValidation(options =>
            {
                options.UseLocalServer();
                options.UseAspNetCore();
            });

        return builder;
    }
}
