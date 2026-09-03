// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using OroIdentityServer.Infraestructure;
using Quartz;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace IdentityServer.Server.Extensions;

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
                    .SetIntrospectionEndpointUris("connect/introspect")
                    .SetRevocationEndpointUris("connect/revoke");

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

                if (builder.Environment.IsDevelopment() || builder.Environment.EnvironmentName.Equals("Testing"))
                    options.AddDevelopmentEncryptionCertificate()
                        .AddDevelopmentSigningCertificate();
                else
                {
                    var cert = LoadOrCreateSigningCertificate(builder.Configuration);
                    options.AddEncryptionCertificate(cert);
                    options.AddSigningCertificate(cert);
                }

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

    private static X509Certificate2 LoadOrCreateSigningCertificate(IConfiguration configuration)
    {
        var certsDir = Path.Combine(AppContext.BaseDirectory, "certificates");
        var certPath = Path.Combine(certsDir, "openiddict-signing.pfx");

        if (File.Exists(certPath))
            return X509CertificateLoader.LoadCertificateFromFile(certPath);

        using var rsa = RSA.Create(2048);
        var cn = configuration["Branding:FullName"] ?? "OroIdentityServer";
        var req = new CertificateRequest($"CN={cn}", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        var cert = req.CreateSelfSigned(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddYears(5));

        Directory.CreateDirectory(certsDir);
        File.WriteAllBytes(certPath, cert.Export(X509ContentType.Pfx));

        return cert;
    }
}
