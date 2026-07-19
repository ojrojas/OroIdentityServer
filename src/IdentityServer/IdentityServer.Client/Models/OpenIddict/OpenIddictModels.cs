namespace IdentityServer.Client.Models.OpenIddict;

/// <summary>
/// Used for both GET responses and POST/PUT request bodies for /api/applications,
/// mirroring the fields the server actually reads off OpenIddictApplicationDescriptor.
/// </summary>
public sealed record OpenIddictApplicationModel(
    string? ClientId,
    string? ClientSecret,
    string? DisplayName,
    string? ClientType,
    string? ApplicationType,
    string? ConsentType,
    List<string>? Permissions,
    List<string>? Requirements,
    List<string>? RedirectUris,
    List<string>? PostLogoutRedirectUris);

/// <summary>
/// GET-only view model for /api/scopes (full descriptor shape used for display).
/// </summary>
public sealed record OpenIddictScopeModel(
    string? Name,
    string? DisplayName,
    string? Description,
    List<string>? Resources);

public sealed record CreateOpenIddictScopeRequest(string Name, IEnumerable<string> Resources);

public sealed record UpdateOpenIddictScopeRequest(string Name, IEnumerable<string> Resources);
