// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.IdentityModel.Logging;

namespace OroIdentity.Web.Server.Extensiones;

public static class OroIdentityWebExtensions
{
    public static TBuilder AddOroIdentityWebExtensions<TBuilder>(
       this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {

        builder.Services.AddAuthentication(opt =>
        {
            opt.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            opt.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        })
        .AddCookie(options =>
        {
            options.LoginPath = "/account/login";
        });
       
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


        IdentityModelEventSource.ShowPII = builder.Environment.IsDevelopment();

        builder.Services.AddHttpContextAccessor();
        return builder;
    }
}