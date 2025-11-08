// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Services.OroIdentityServer.Api.Extensions;

public static class AppExtensions
{
    public static void AddAppExtensions(this IHostApplicationBuilder builder)
    {
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<IIdentityRedirectService,IdentityRedirectService>();
    }
}