namespace OroIdentityServer.Shared.Authorization;

/// <summary>
/// Claim type names shared between the IdP server (which writes them into the auth
/// cookie) and the Blazor client (which reads them from the AuthenticationState).
/// Both projects reference OroIdentityServer.Shared, so this is the only place to
/// keep them in sync.
/// </summary>
public static class AuthorizationClaimTypes
{
    /// <summary>
    /// "true" when the signed-in user is the master admin: an Admin in the seed
    /// tenant (the tenant whose name matches <c>SEED_TENANT_NAME</c>, default
    /// <c>OroMasterTenant</c>). Drives the <c>MasterAdminOnly</c> authorization
    /// policy and the OIDC/tenant sections of the web console.
    /// </summary>
    public const string IsMasterAdmin = "is_master_admin";
}
