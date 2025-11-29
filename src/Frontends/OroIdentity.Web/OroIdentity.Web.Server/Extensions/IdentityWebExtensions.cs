using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.IdentityModel.Logging;

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
                config.LoginPath="/Account/Login";
            }
            );
        builder.Services.AddAuthorization();

        IdentityModelEventSource.ShowPII = true;

        builder.Services.AddHttpContextAccessor();
        return builder;
    }
}