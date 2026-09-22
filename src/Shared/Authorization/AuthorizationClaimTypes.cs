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

    /// <summary>
    /// The home tenant of the signed-in user. Written into the admin cookie and into the
    /// OIDC access/identity tokens issued by the authorization endpoint so relying party
    /// applications can scope their data by tenant without calling userinfo.
    /// </summary>
    public const string TenantId = "tenant_id";

    /// <summary>
    /// A domain user permission granted to the signed-in user through their active roles.
    /// One claim is emitted per permission and its value is the permission name
    /// (<c>Provider.Resource.Action</c>, e.g. <c>oropos.sales.read</c>).
    /// </summary>
    /// <remarks>
    /// This follows the standard IdentityModel claim behavior (a fixed claim type with one
    /// value per grant), so <c>RequireClaim("permission", "oropos.sales.read")</c> and
    /// <c>User.HasClaim("permission", "oropos.sales.read")</c> work as expected.
    /// Do NOT confuse this with an OpenIddict <b>client</b> permission
    /// (<c>ept:</c>, <c>gt:</c>, <c>rst:</c>, <c>scp:</c>, <c>ft:</c>), which is stored on the
    /// client application and describes what the client may do, not what the user may do.
    /// Matching is exact; no wildcards.
    /// </remarks>
    public const string Permission = "permission";
}
