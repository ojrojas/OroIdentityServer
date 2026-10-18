// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using IdentityServer.Services;
using OroIdentityServer.Infraestructure;
using OroIdentityServer.Server.Jobs;
using OroIdentityServer.Shared.Authorization;
using OpenIddict.Server;
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

            // Sweep for sessions that timed out and owe their owner a notification.
            options.AddJob<ExpiredSessionNotificationJob>(job =>
                job.WithIdentity(ExpiredSessionNotificationJob.JobName).StoreDurably());

            options.AddTrigger(trigger => trigger
                .ForJob(ExpiredSessionNotificationJob.JobName)
                .WithIdentity($"{ExpiredSessionNotificationJob.JobName}-trigger")
                .StartNow()
                .WithSimpleSchedule(schedule => schedule
                    .WithInterval(TimeSpan.FromMinutes(5))
                    .RepeatForever()));
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
                    AuthorizationClaimTypes.TenantId,
                    AuthorizationScopes.Permissions
                    );

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

                // Introspection is answered by OpenIddict's own pipeline (this version exposes no
                // passthrough for it), but its built-in claim attachment withholds the application
                // claims from anyone who is not an audience of the token. This contributor re-adds
                // them, so an authorised introspector sees what the token actually says. See
                // IntrospectionClaimsContributor.
                options.AddEventHandler<OpenIddictServerEvents.HandleIntrospectionRequestContext>(builder =>
                    builder.UseScopedHandler<IntrospectionClaimsOpenIddictHandler>());
            })
            .AddValidation(options =>
            {
                options.UseLocalServer();
                options.UseAspNetCore();

                // Without this, validation only checks the token's signature/expiry: a token that
                // was revoked in the database (logout, "sign out everywhere", remote logout) keeps
                // authorising /api calls until it expires. Introspection already consults the store,
                // so leaving this off made the two paths disagree - introspection said
                // "active: false" while /api happily served the same token. Costs one token lookup
                // per request, which is the price of honouring revocation.
                options.EnableTokenEntryValidation();
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
