// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using Microsoft.EntityFrameworkCore;

namespace OroIdentity.Web.Server.Extensiones;

public static class AppWebExtensions
{
    public static TBuilder AddAppWebExtensions<TBuilder>(
       this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.Services.AddDbContext<DbContext>(options =>
        {
            options.UseSqlite($"Filename={Path.Combine(builder.Environment.ContentRootPath, "orodientityserver-admin.db")}");
            options.UseOpenIddict();
        });

        return builder;
    }
}
