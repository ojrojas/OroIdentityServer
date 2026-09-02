namespace IdentityServer.Client.Models.Roles;

public sealed record RoleModel(Guid Id, bool IsActive, string? Name, IEnumerable<RolePermissionModel>? Claims, DateTime CreatedAtUtc, int Level, Guid? ParentRoleId);

public sealed record RolePermissionModel(Guid RoleId, Guid PermissionId);

public sealed record CreateRoleRequest(string RoleName, int Level = 10, Guid? ParentRoleId = null);

public sealed record UpdateRoleRequest(string RoleName, int Level, Guid? ParentRoleId);
