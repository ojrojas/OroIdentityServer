namespace IdentityServer.Client.Models.OpenIddict;

/// <summary>
/// Option lists used to configure OpenIddict application permissions in the admin UI.
/// The values are the raw OpenIddict permission strings stored on the application
/// (ept:*, gt:*, rst:*, scp:*, ft:*). Verified against OpenIddict.Abstractions 8.0.0-preview.2
/// (OpenIddictConstants.Permissions / OpenIddictConstants.Requirements).
/// </summary>
public static class OpenIddictPermissionOptions
{
    public sealed record Option(string Value, string Label);

    public static readonly Option[] Endpoints =
    [
        new("ept:authorization", "Authorization"),
        new("ept:token", "Token"),
        new("ept:end_session", "End session"),
        new("ept:introspection", "Introspection"),
        new("ept:revocation", "Revocation"),
    ];

    public static readonly Option[] GrantTypes =
    [
        new("gt:authorization_code", "Authorization code"),
        new("gt:client_credentials", "Client credentials"),
        new("gt:refresh_token", "Refresh token"),
        new("gt:password", "Password"),
    ];

    public static readonly Option[] ResponseTypes =
    [
        new("rst:code", "Code"),
        new("rst:id_token", "Id token"),
        new("rst:token", "Token"),
        new("rst:id_token token", "Id token + token"),
    ];

    public static readonly Option[] Scopes =
    [
        new("scp:openid", "openid"),
        new("scp:profile", "profile"),
        new("scp:email", "email"),
        new("scp:roles", "roles"),
        new("scp:offline_access", "offline_access"),
        new("scp:admin", "admin"),
    ];

    public static readonly Option[] Requirements =
    [
        new("ft:pkce", "Proof Key for Code Exchange (PKCE)"),
    ];

    /// <summary>
    /// Splits the stored permission strings into the standard checkbox sets and any
    /// custom scope permissions (e.g. scp:tenant), which are returned without the "scp:" prefix.
    /// </summary>
    public static IEnumerable<string> CustomScopeNames(IEnumerable<string> permissions)
        => permissions
            .Where(p => p.StartsWith("scp:", StringComparison.Ordinal))
            .Where(p => !Scopes.Any(s => s.Value == p))
            .Select(p => p[4..])
            .OrderBy(name => name, StringComparer.OrdinalIgnoreCase);
}
