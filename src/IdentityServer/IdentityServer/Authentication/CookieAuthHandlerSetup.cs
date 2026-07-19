// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using Microsoft.AspNetCore.Authentication.Cookies;

namespace OroIdentityServer.Server.Authentication;

public static class CookieAuthHandlerSetup
{
    public const string AdminScheme = CookieAuthenticationDefaults.AuthenticationScheme;

    public static IServiceCollection AddAdminAuthentication(this IServiceCollection services)
    {
        services.AddAuthentication(AdminScheme)
            .AddCookie(AdminScheme, options =>
            {
                options.LoginPath = "/Account/Login";
                options.LogoutPath = "/Account/Logout";
                options.AccessDeniedPath = "/access-denied";
                options.ExpireTimeSpan = TimeSpan.FromHours(8);
                options.SlidingExpiration = true;
                options.Cookie.Name = "oro.identity.admin";
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;

                // Requests under /api are fetched by the Blazor client's HttpClient connectors,
                // not navigated to by the browser. Redirecting them to the HTML login page (the
                // cookie handler's default challenge/forbid behavior) makes the client try to
                // deserialize that HTML as JSON. Return a plain status code for those instead.
                options.Events.OnRedirectToLogin = context =>
                {
                    if (context.Request.Path.StartsWithSegments("/api"))
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        return Task.CompletedTask;
                    }

                    context.Response.Redirect(context.RedirectUri);
                    return Task.CompletedTask;
                };
                options.Events.OnRedirectToAccessDenied = context =>
                {
                    if (context.Request.Path.StartsWithSegments("/api"))
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        return Task.CompletedTask;
                    }

                    context.Response.Redirect(context.RedirectUri);
                    return Task.CompletedTask;
                };
            });

        return services;
    }

    public static IServiceCollection AddAdminAuthorization(this IServiceCollection services)
    {
        services.AddAuthorizationBuilder()
            .AddPolicy("AdminOnly", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireRole("Administrator");
            });
            
        return services;
    }
}
