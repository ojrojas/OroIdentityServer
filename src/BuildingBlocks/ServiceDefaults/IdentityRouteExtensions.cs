namespace OroIdentityServer.BuildingBlocks.ServicesDefaults;

public class IdentityEndpointOptions
{
    public string LoginPath { get; set; } = "/account/login";
    public string LogoutPath { get; set; } = "/account/logout";
    public string CallbackPath { get; set; } = "/signin-oidc";
    public string SignoutCallbackPath { get; set; } = "/signout-callback-oidc";
    public string DefaultRedirectUri { get; set; } = "/";
}

public static class IdentityRouteExtensions
{
    public static IEndpointRouteBuilder MapIdentityEndpoints(this IEndpointRouteBuilder app, Action<IdentityEndpointOptions>? configureOptions = null)
    {
        var options = new IdentityEndpointOptions();
        configureOptions?.Invoke(options);

        // Endpoint de Login (Challenge)
        app.MapGet(options.LoginPath, (string? returnUrl) =>
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = returnUrl ?? options.DefaultRedirectUri
            };
            return Results.Challenge(properties, [OpenIddictClientAspNetCoreDefaults.AuthenticationScheme]);
        });

        // Endpoint de Logout (SignOut)
        app.MapGet(options.LogoutPath, (string? returnUrl, HttpContext context) =>
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = returnUrl ?? options.DefaultRedirectUri
            };

            return Results.SignOut(properties, [
                CookieAuthenticationDefaults.AuthenticationScheme,
                OpenIddictClientAspNetCoreDefaults.AuthenticationScheme
            ]);
        });

        // Endpoint de Logout redirection
        app.MapPost(options.LogoutPath, (string? returnUrl) =>
            Results.Redirect($"{options.LogoutPath}?returnUrl={Uri.EscapeDataString(returnUrl ?? options.DefaultRedirectUri)}"));

        // Endpoint Callback (signin-oidc)
        app.MapMethods(options.CallbackPath, [HttpMethods.Get, HttpMethods.Post], async (HttpContext context) =>
        {
            var result = await context.AuthenticateAsync(OpenIddictClientAspNetCoreDefaults.AuthenticationScheme);
            if (!result.Succeeded || result.Principal == null)
            {
                return Results.Problem("External authentication failed.");
            }

            // Sign in the user with a cookie.
            var identity = new ClaimsIdentity(result.Principal.Claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, result.Properties ?? new AuthenticationProperties());

            var redirectUri = result.Properties?.RedirectUri;
            if (string.IsNullOrEmpty(redirectUri) || !Uri.IsWellFormedUriString(redirectUri, UriKind.RelativeOrAbsolute))
            {
                redirectUri = options.DefaultRedirectUri;
            }

            return Results.Redirect(redirectUri);
        }).DisableAntiforgery();

        // Endpoint Signout Callback (signout-callback-oidc)
        app.MapMethods(options.SignoutCallbackPath, [HttpMethods.Get, HttpMethods.Post], async (HttpContext context) =>
        {
            var result = await context.AuthenticateAsync(OpenIddictClientAspNetCoreDefaults.AuthenticationScheme);
            
            var redirectUri = result.Properties?.RedirectUri;
            if (string.IsNullOrEmpty(redirectUri) || !Uri.IsWellFormedUriString(redirectUri, UriKind.RelativeOrAbsolute))
            {
                redirectUri = options.DefaultRedirectUri;
            }

            return Results.Redirect(redirectUri);
        }).DisableAntiforgery();

        return app;
    }

    private static AuthenticationProperties GetAuthProperties(string? returnUrl)
    {
        // TODO: Use HttpContext.Request.PathBase instead.
        const string pathBase = "/";

        // Prevent open redirects.
        if (string.IsNullOrEmpty(returnUrl))
        {
            returnUrl = pathBase;
        }
        else if (!Uri.IsWellFormedUriString(returnUrl, UriKind.Relative))
        {
            returnUrl = new Uri(returnUrl, UriKind.Absolute).PathAndQuery;
        }
        else if (returnUrl[0] != '/')
        {
            returnUrl = $"{pathBase}{returnUrl}";
        }

        return new AuthenticationProperties { RedirectUri = returnUrl };
    }
}


