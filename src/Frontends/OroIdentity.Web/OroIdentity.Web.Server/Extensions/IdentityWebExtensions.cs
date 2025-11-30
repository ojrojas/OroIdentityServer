using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.IdentityModel.Logging;
using Polly;

namespace OroIdentity.Web.Server.Extensiones;

public static class OroIdentityWebExtensions
{
    public static TBuilder AddOroIdentityWebExtensions<TBuilder>(
       this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {

        builder.Services.AddAuthentication(
              CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(
                config =>
            {
                config.LoginPath = "/Account/Login";
            }
            );
        builder.Services.AddAuthorizationBuilder()
            .AddPolicy("CanAccess", policy =>
            {
                policy.RequireAuthenticatedUser()
                .RequireRole("Administrator","User").Build();
            })
            .AddPolicy("CanSeeError", policy =>
            {
                policy.RequireAuthenticatedUser()
                 .RequireRole("Administrator","User").Build();
            });


        IdentityModelEventSource.ShowPII = true;

        builder.Services.AddHttpContextAccessor();
        return builder;
    }
}