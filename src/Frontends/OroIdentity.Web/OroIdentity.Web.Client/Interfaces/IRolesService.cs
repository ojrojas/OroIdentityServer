using OroIdentity.Web.Client.Models;

namespace OroIdentity.Web.Client.Interfaces;


public interface IRolesService
{
    Task<BaseResponseViewModel<IEnumerable<RoleViewModel>>> GetAllRolesAsync(CancellationToken cancellationToken);
    Task<RoleViewModel> GetRoleByNameAsync(string roleName, CancellationToken cancellationToken);
    Task<RoleViewModel> GetRoleByIdAsync(string roleId, CancellationToken cancellationToken);
    Task CreateRoleAsync(RoleViewModel role, CancellationToken cancellationToken);
    Task UpdateRoleAsync(RoleViewModel role, CancellationToken cancellationToken);
    Task DeleteRoleAsync(string roleId, CancellationToken cancellationToken); 
}