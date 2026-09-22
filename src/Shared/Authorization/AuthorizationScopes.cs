namespace OroIdentityServer.Shared.Authorization;

/// <summary>
/// OAuth/OIDC scope names used by OroIdentityServer in addition to the standard
/// <c>OpenIddictConstants.Scopes</c>. These are the scopes clients can request and that
/// the server registers during OpenIddict configuration.
/// </summary>
public static class AuthorizationScopes
{
    /// <summary>
    /// Grants the <c>permission</c> claims of the signed-in user to the identity token and the
    /// <c>userinfo</c> response. The access token always carries the <c>permission</c> claims,
    /// regardless of this scope, so resource servers can enforce authorization directly.
    /// </summary>
    /// <remarks>
    /// Do not confuse this with an OpenIddict <b>client</b> permission: the corresponding
    /// client permission string stored on the application is <c>scp:permissions</c>.
    /// </remarks>
    public const string Permissions = "permissions";
}
