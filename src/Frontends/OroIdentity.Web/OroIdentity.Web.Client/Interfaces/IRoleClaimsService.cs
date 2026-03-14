using OroIdentity.Web.Client.Models;

namespace OroIdentity.Web.Client.Interfaces;

public interface IRoleClaimsService
{
    Task<RoleClaimViewModel?> GetRoleClaimByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<BaseResponseViewModel<IEnumerable<RoleClaimViewModel>>> GetRoleClaimsByRoleIdAsync(Guid roleId, CancellationToken cancellationToken);
    Task AssociateClaimToRoleAsync(Guid roleId, string claimType, string claimValue, CancellationToken cancellationToken);
    Task AssociatePermissionsToRoleAsync(Guid roleId, IEnumerable<Guid> permissionIds, CancellationToken cancellationToken);
    Task UpdateRoleClaimAsync(RoleClaimViewModel claim, CancellationToken cancellationToken);
    Task DeleteRoleClaimAsync(Guid id, CancellationToken cancellationToken);
}
