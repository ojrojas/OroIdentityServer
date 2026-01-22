// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using OpenIddict.Client.AspNetCore;

namespace OroIdentityServer.Services.OroIdentityServer.Server.Extensions;

public static class IdentityApiExtensions
{
    public static void AddIdentityApiExtensions(
        this IHostApplicationBuilder builder, IConfiguration configuration)
    {
        builder.Services.AddAuthentication(opt =>
       {
           opt.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
           opt.DefaultChallengeScheme = OpenIddictClientAspNetCoreDefaults.AuthenticationScheme;
       })
        .AddCookie(
           config =>
        {
            config.LoginPath = "/Account/Login";
            config.SlidingExpiration = true;
            config.ExpireTimeSpan = TimeSpan.FromHours(8);
        }
        );

        builder.Services.AddAuthorization();
    }
}
