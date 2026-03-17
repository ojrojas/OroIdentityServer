// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Client;
using Quartz;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace OroIdentity.Web.Server.Extensiones;

public static class OpenIddictExtensions
{
    public static IServiceCollection AddDIOpenIddictApplication(
        this IServiceCollection services, IConfiguration configuration)
    {
        var signingKey = configuration.GetSection("SymmetricSecurityKey").Value;

        ArgumentException.ThrowIfNullOrEmpty(signingKey, "SymmetricSecurityKey configuration is missing");

        // OpenIddict offers native integration with Quartz.NET to perform scheduled tasks
        // (like pruning orphaned authorizations from the database) at regular intervals.
        services.AddQuartz(options =>
        {
            options.UseSimpleTypeLoader();
            options.UseInMemoryStore();
        });

        // Register the Quartz.NET service and configure it to block shutdown until jobs are complete.
        services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

        services.AddOpenIddict()

            .AddCore(options =>
            {
                options.UseEntityFrameworkCore()
                .UseDbContext<DbContext>();

                options.UseQuartz();
            })

            .AddClient(options =>
            {
                // Note: this sample uses the authorization code flow,
                // but you can enable the other flows if necessary.
                options.AllowAuthorizationCodeFlow()
                       .AllowRefreshTokenFlow();

                options.SetPostLogoutRedirectionEndpointUris(
                    new Uri($"{configuration["IdentityWeb:Url"]}/signout-callback-oidc"));

                // Register the signing and encryption credentials used to protect
                // sensitive data like the state tokens produced by OpenIddict.
                // Configure encryption and signing of tokens.  testing phrase tokens ORO_IDENTITY_SERVER_PROJECT_0001
                options.AddEncryptionKey(new SymmetricSecurityKey(
                    Convert.FromBase64String(signingKey)));

                // Add a development signing certificate for interactive flows
                options.AddDevelopmentSigningCertificate();


                options.UseAspNetCore()
                    .EnableStatusCodePagesIntegration()
                       .EnableRedirectionEndpointPassthrough()
                       .EnablePostLogoutRedirectionEndpointPassthrough();
               
                options.UseSystemNetHttp()
                       .SetProductInformation(typeof(Program).Assembly);

                options.AddRegistration(new OpenIddictClientRegistration
                {
                    ClientId = "OroIdentityServer.Web",
                    RedirectUri = new Uri($"{configuration["IdentityWeb:Url"]}/signin-oidc"),
                    PostLogoutRedirectUri = new Uri($"{configuration["IdentityWeb:Url"]}/signout-callback-oidc"),
                    Issuer = new Uri($"{configuration["Identity:Url"]}"),
                    ClientSecret = "a2344152-e928-49e7-bb3c-ee54acc96c8c",
                    Scopes =
                    {
                        Scopes.OpenId,
                        Scopes.Email,
                        Scopes.Profile,
                        Scopes.Roles,
                    },
                    GrantTypes =
                    {
                        GrantTypes.AuthorizationCode,
                        GrantTypes.RefreshToken,
                    },
                });

                // // Log token request parameters for debugging (temporary)
                // options.AddEventHandler<ApplyTokenRequestContext>(builder =>
                // {
                //     builder.UseInlineHandler(context =>
                //     {
                //         try
                //         {
                //             var reqProp = context.GetType().GetProperty("Request");
                //             var req = reqProp?.GetValue(context);
                //             if (req != null)
                //             {
                //                 var paramsProp = req.GetType().GetProperty("Parameters");
                //                 if (paramsProp?.GetValue(req) is System.Collections.Generic.IDictionary<string, string> parameters)
                //                 {
                //                     Console.WriteLine("=== Token request parameters ===");
                //                     foreach (var kv in parameters)
                //                         Console.WriteLine($"{kv.Key}: {kv.Value}");
                //                     Console.WriteLine("=== End token request parameters ===");
                //                 }
                //             }
                //         }
                //         catch (Exception ex)
                //         {
                //             Console.WriteLine("Error logging token request: " + ex);
                //         }

                //         return default;
                //     });
                // });
            });

        return services;
    }
}