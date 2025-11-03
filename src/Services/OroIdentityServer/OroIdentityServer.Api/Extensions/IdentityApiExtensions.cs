// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Services.OroIdentityServer.Api.Extensions;

public static class IdentityApiExtensions
{
    public static void AddIdentityApiExtensions(this IHostApplicationBuilder builder, IConfiguration configuration)
    {
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        });

        builder.Services.AddAuthorizationBuilder()
            .AddPolicy("CookieAuthenticationPolicy", builder =>
        {
            builder.AddAuthenticationSchemes(CookieAuthenticationDefaults.AuthenticationScheme);
            builder.RequireAuthenticatedUser();
        });

        builder.Services.AddQuartz(options =>
        {
            options.UseSimpleTypeLoader();
            options.UseInMemoryStore();
        });

        builder.Services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);
    }
}