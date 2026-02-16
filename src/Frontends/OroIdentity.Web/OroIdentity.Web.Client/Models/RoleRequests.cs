namespace OroIdentity.Web.Client.Models;

public record CreateRoleRequest(string Name, bool IsActive);

public record UpdateRoleRequest(string Name, bool IsActive);

public record GetRolesResponse(IEnumerable<RoleViewModel>? Data = null);

public record GetRoleResponse(RoleViewModel? Data = null);

public record CreateRoleResponse(Guid? Id = null);

public record UpdateRoleResponse;