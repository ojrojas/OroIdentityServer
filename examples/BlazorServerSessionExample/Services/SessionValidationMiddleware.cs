using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using BlazorServerSessionExample.Services;

namespace BlazorServerSessionExample.Services;

/// <summary>
/// Middleware that validates the user's session on every request by introspecting
/// the access token against the Identity Server. If the token has been revoked
/// (e.g. admin logged the user out remotely), the user is signed out immediately.
/// </summary>
public sealed class SessionValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SessionValidationMiddleware> _logger;

    public SessionValidationMiddleware(RequestDelegate next, ILogger<SessionValidationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path;
        if (IsExemptPath(path))
        {
            await _next(context);
            return;
        }

        if (context.User.Identity?.IsAuthenticated != true)
        {
            await _next(context);
            return;
        }

        var token = await GetAccessTokenAsync(context);
        if (string.IsNullOrEmpty(token))
        {
            await _next(context);
            return;
        }

        var monitor = context.RequestServices.GetRequiredService<SessionMonitor>();
        if (!await monitor.IsTokenActiveAsync(token, context.RequestAborted))
        {
            _logger.LogWarning("Token revoked or expired for user {User}, signing out",
                context.User.FindFirstValue(ClaimTypes.Name));

            await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            context.Response.Redirect("/Account/Login");
            return;
        }

        await _next(context);
    }

    private static bool IsExemptPath(PathString path)
    {
        return path.StartsWithSegments("/Account")
            || path.StartsWithSegments("/auth")
            || path.StartsWithSegments("/connect")
            || path.StartsWithSegments("/api")
            || path.StartsWithSegments("/_blazor")
            || path.StartsWithSegments("/_framework")
            || path.StartsWithSegments("/css")
            || path.StartsWithSegments("/js");
    }

    private static async Task<string?> GetAccessTokenAsync(HttpContext context)
    {
        if (context.Request.Cookies.TryGetValue("access_token", out var cookieToken))
            return cookieToken;

        var result = await context.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        if (result.Properties?.Items.TryGetValue(".Token.access_token", out var tokenValue) == true)
            return tokenValue;

        return null;
    }
}
