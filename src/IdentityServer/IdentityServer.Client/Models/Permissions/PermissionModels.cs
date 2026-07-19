namespace IdentityServer.Client.Models.Permissions;

public sealed record PermissionModel(
    Guid PermissionId,
    string Provider,
    string? Description,
    string Action,
    string Resource,
    bool IsSystem);

public sealed record CreatePermissionRequest(
    Guid PermissionId,
    string Provider,
    string? Description,
    string Action,
    string Resource,
    bool IsSystem);

public sealed record UpdatePermissionRequest(
    Guid PermissionId,
    string Provider,
    string? Description,
    string Action,
    string Resource,
    bool IsSystem);
