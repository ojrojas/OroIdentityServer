
// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Application.Extensions;

public static class ApplicationExtensions
{
    public static void AddApplicationExtensions(this IHostApplicationBuilder builder, IConfiguration configuration)
    {
        builder.Services.AddCqrsHandlers();
    }
}