using OroIdentity.Web.Client.Models;

namespace OroIdentity.Web.Client.Interfaces;

public interface IPermissionsService
{
    Task<BaseResponseViewModel<IEnumerable<PermissionViewModel>>> GetAllPermissionsAsync(CancellationToken cancellationToken);
    Task<PermissionViewModel?> GetPermissionByIdAsync(Guid permissionId, CancellationToken cancellationToken);
    Task CreatePermissionAsync(PermissionViewModel permission, CancellationToken cancellationToken);
    Task UpdatePermissionAsync(PermissionViewModel permission, CancellationToken cancellationToken);
    Task DeletePermissionAsync(Guid permissionId, CancellationToken cancellationToken);
}
