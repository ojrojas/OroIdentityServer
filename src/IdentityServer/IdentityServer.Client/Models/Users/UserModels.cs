namespace IdentityServer.Client.Models.Users;

public sealed record UserModel(
    Guid Id,
    string? Name,
    string? MiddleName,
    string? LastName,
    string? UserName,
    string? Email,
    string? Identification,
    Guid? IdentificationTypeId,
    string? NormalizedEmail,
    string? NormalizedUserName,
    Guid? TenantId,
    Guid? SecurityUserId,
    bool IsActive,
    bool IsLocked,
    DateTime? LockoutEnd,
    IReadOnlyCollection<UserRoleModel> Roles,
    IReadOnlyCollection<UserPermissionModel> Permissions,
    DateTime CreatedAtUtc);

public sealed record UserRoleModel(Guid? UserId, Guid? RoleId);

public sealed record UserPermissionModel(Guid? UserId, Guid? PermissionId);

public sealed record CreateUserRequest(
    string Name,
    string MiddleName,
    string LastName,
    string UserName,
    string Email,
    string Password,
    string Identification,
    Guid IdentificationTypeId,
    Guid TenantId);

public sealed record UpdateUserRequest(
    string Name,
    string MiddleName,
    string LastName,
    string UserName,
    string Email,
    string? Password,
    string Identification,
    Guid IdentificationTypeId,
    Guid TenantId);

public sealed record AssignRolesRequest(List<Guid> RoleIds);

public sealed record AssignPermissionsRequest(List<Guid> PermissionIds);
