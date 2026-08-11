// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using Microsoft.AspNetCore.Authentication.Cookies;
using OroIdentityServer.Core.Modules.Tenants.ValueObjects;

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
                // deserialize that HTML as JSON. Return a plain status code for those instead, and
                // write a body so the status code pages middleware does not re-execute the request
                // (which would run POSTs against /not-found and trip the antiforgery middleware).
                options.Events.OnRedirectToLogin = async context =>
                {
                    if (context.Request.Path.StartsWithSegments("/api"))
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsync("{\"error\":\"unauthorized\"}");
                        return;
                    }

                    context.Response.Redirect(context.RedirectUri);
                };
                options.Events.OnRedirectToAccessDenied = async context =>
                {
                    if (context.Request.Path.StartsWithSegments("/api"))
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsync("{\"error\":\"forbidden\"}");
                        return;
                    }

                    context.Response.Redirect(context.RedirectUri);
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
                policy.RequireRole(TenantRole.Admin, TenantRole.Administrator);
            })
            .AddPolicy("ManagerOrAdmin", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireRole(TenantRole.Admin, TenantRole.Administrator, TenantRole.Manager);
            });

        return services;
    }
}
