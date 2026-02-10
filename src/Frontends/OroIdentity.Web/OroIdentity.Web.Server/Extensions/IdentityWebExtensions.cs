// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.IdentityModel.Logging;
using OpenIddict.Client.AspNetCore;

namespace OroIdentity.Web.Server.Extensiones;

public static class OroIdentityWebExtensions
{
    public static TBuilder AddOroIdentityWebExtensions<TBuilder>(
       this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {

        builder.Services.AddAuthentication(opt =>
        {
            opt.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            opt.DefaultChallengeScheme = OpenIddictClientAspNetCoreDefaults.AuthenticationScheme;
        })

        .AddBearerToken()

        .AddCookie(options =>
        {
            options.LoginPath = "/account/login";
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.ExpireTimeSpan = TimeSpan.FromDays(7);
        });
        
        builder.Services.AddAuthorizationBuilder()
            .AddPolicy("CanAccess", policy =>
            {
                policy.RequireAuthenticatedUser()
                .RequireRole("Administrator", "User").Build();
            })
            .AddPolicy("CanSeeError", policy =>
            {
                policy.RequireAuthenticatedUser()
                 .RequireRole("Administrator", "User").Build();
            });

        builder.Services.AddAuthorization();
        builder.Services.AddCascadingAuthenticationState();
        builder.Services.AddAuthenticationStateDeserialization();

        IdentityModelEventSource.ShowPII = builder.Environment.IsDevelopment();

        builder.Services.AddHttpContextAccessor();

        return builder;
    }
}