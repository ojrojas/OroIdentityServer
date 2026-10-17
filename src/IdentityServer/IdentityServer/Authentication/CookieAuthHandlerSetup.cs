// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Authorization;
using OroIdentityServer.Core.Modules.Tenants.ValueObjects;
using OroIdentityServer.Server.Authorization;
using OroIdentityServer.Shared.Authorization;

namespace OroIdentityServer.Server.Authentication;

public static class CookieAuthHandlerSetup
{
    public const string AdminScheme = CookieAuthenticationDefaults.AuthenticationScheme;

    /// <summary>
    /// Scheme that carries the "password accepted, second factor pending" state. It is a
    /// separate scheme from <see cref="AdminScheme"/> on purpose: the ticket only holds an
    /// opaque challenge token, and the per-scheme data protection discriminator keeps the two
    /// ticket formats from being interchangeable.
    /// </summary>
    public const string MfaPendingScheme = "MfaPending";

    public const string MfaChallengeTokenClaimType = "mfa_challenge";

    /// <summary>
    /// How long a session lasts without activity. Shared by the cookie, the <c>UserSession</c>
    /// row and the sliding refresh in the session middleware, so the two lifetimes cannot drift
    /// apart (a cookie that outlives its row, or the other way round, is a support incident).
    /// </summary>
    public static readonly TimeSpan SessionLifetime = TimeSpan.FromHours(8);

    /// <summary>
    /// Key purposes that keep the two cookie schemes' tickets mutually unreadable. Without them
    /// a pending-MFA ticket would be a valid ticket for the admin scheme too.
    /// </summary>
    private const string AdminKeyPurpose = "oro.identity.admin.v1";
    private const string MfaPendingKeyPurpose = "oro.identity.mfa.pending.v1";

    public static IServiceCollection AddAdminAuthentication(this IServiceCollection services)
    {
        services.AddAuthentication(AdminScheme)
            .AddCookie(MfaPendingScheme, options =>
            {
                options.Cookie.Name = "oro.identity.mfa.pending";
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                options.Cookie.Path = "/";
                // A challenge is only good for a couple of minutes: the code windows are 30
                // seconds each, so there is no reason to keep the pending state alive longer.
                options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
                options.SlidingExpiration = false;
                options.LoginPath = "/Account/Login";
            })
            .AddCookie(AdminScheme, options =>
            {
                options.LoginPath = "/Account/Login";
                options.LogoutPath = "/Account/Logout";
                options.AccessDeniedPath = "/access-denied";
                options.ExpireTimeSpan = SessionLifetime;
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
                        // The Location header lets the WASM client notice the expired session
                        // and send the user to the login page instead of rendering a broken page.
                        context.Response.Headers.Location = "/Account/Login?reason=expired";
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

        // Applied after AddCookie so these win over the handler defaults. Resolved from DI
        // (not built eagerly) to keep the Data Protection registration order irrelevant.
        services.AddOptions<CookieAuthenticationOptions>(AdminScheme)
            .Configure<Microsoft.AspNetCore.DataProtection.IDataProtectionProvider>((options, provider) =>
                options.DataProtectionProvider = new PurposeDataProtectionProvider(provider, AdminKeyPurpose));

        services.AddOptions<CookieAuthenticationOptions>(MfaPendingScheme)
            .Configure<Microsoft.AspNetCore.DataProtection.IDataProtectionProvider>((options, provider) =>
                options.DataProtectionProvider = new PurposeDataProtectionProvider(provider, MfaPendingKeyPurpose));

        return services;
    }

    /// <summary>Marks the browser as "second factor pending" for <paramref name="challengeToken"/>.</summary>
    public static Task StartMfaChallengeAsync(HttpContext httpContext, string challengeToken)
    {
        var identity = new ClaimsIdentity(
            [new Claim(MfaChallengeTokenClaimType, challengeToken)],
            MfaPendingScheme);

        return httpContext.SignInAsync(MfaPendingScheme, new ClaimsPrincipal(identity));
    }

    /// <summary>Reads the pending challenge token, or null when there is none (or it expired).</summary>
    public static async Task<string?> GetPendingMfaChallengeTokenAsync(HttpContext httpContext)
    {
        var result = await httpContext.AuthenticateAsync(MfaPendingScheme);
        return result.Succeeded
            ? result.Principal?.FindFirstValue(MfaChallengeTokenClaimType)
            : null;
    }

    public static Task ClearMfaChallengeAsync(HttpContext httpContext)
        => httpContext.SignOutAsync(MfaPendingScheme);

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
            })
            .AddPolicy("MasterAdminOnly", policy =>
            {
                // Global OIDC catalogue (applications, scopes) and tenant creation are reserved for
                // the master admin: a user with the Administrator/Admin UserRole, or Admin of the
                // SEED_TENANT_NAME. Tenant admins must not see or modify these resources.
                policy.RequireAuthenticatedUser();
                policy.RequireAssertion(ctx => ctx.User.HasClaim(
                    AdminPasswordSignInService.IsMasterAdminClaimType, "true"));
            })
            .AddPolicy("CanManageHierarchy", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireAssertion(ctx =>
                {
                    var levelClaim = ctx.User.FindFirstValue(HierarchyClaimTypes.HierarchyLevel);
                    if (int.TryParse(levelClaim, out var level))
                        return level >= 70;
                    // Fallback to role check
                    return ctx.User.IsInRole(TenantRole.Administrator) || ctx.User.IsInRole(TenantRole.Manager) || ctx.User.IsInRole(TenantRole.Admin);
                });
            })
            .AddPolicy("CanViewSubordinates", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireAssertion(ctx =>
                {
                    var levelClaim = ctx.User.FindFirstValue(HierarchyClaimTypes.HierarchyLevel);
                    if (int.TryParse(levelClaim, out var level))
                        return level >= 40;
                    return ctx.User.IsInRole(TenantRole.Administrator) || ctx.User.IsInRole(TenantRole.Manager) || ctx.User.IsInRole(TenantRole.Admin);
                });
            })
            .AddPolicy("CanLeadProject", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireAssertion(ctx =>
                {
                    var levelClaim = ctx.User.FindFirstValue(HierarchyClaimTypes.HierarchyLevel);
                    if (int.TryParse(levelClaim, out var level))
                        return level >= 60;
                    return ctx.User.IsInRole(TenantRole.Administrator) || ctx.User.IsInRole(TenantRole.Manager) || ctx.User.IsInRole(TenantRole.Admin);
                });
            })
            .AddPolicy("CanAssignMatrixRelationships", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireAssertion(ctx =>
                {
                    var levelClaim = ctx.User.FindFirstValue(HierarchyClaimTypes.HierarchyLevel);
                    if (int.TryParse(levelClaim, out var level))
                        return level >= 60;
                    return ctx.User.IsInRole(TenantRole.Administrator) || ctx.User.IsInRole(TenantRole.Manager) || ctx.User.IsInRole(TenantRole.Admin);
                });
            });

        services.AddScoped<IAuthorizationHandler, HierarchyAuthorizationHandler>();

        // Domain user permissions: exact-match policies named "perm:<permission-name>".
        services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();

        return services;
    }
}
