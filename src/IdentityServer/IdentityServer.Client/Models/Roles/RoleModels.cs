namespace IdentityServer.Client.Models.Roles;

public sealed record RoleModel(Guid Id, bool IsActive, string? Name, IEnumerable<RolePermissionModel>? Claims);

public sealed record RolePermissionModel(Guid RoleId, Guid PermissionId);

public sealed record CreateRoleRequest(string RoleName);

public sealed record UpdateRoleRequest(string RoleName);
