// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.OroIdentityServer.Infraestructure.Repositories;

public class RolesRepository(
    ILogger<RolesRepository> logger, 
    IRepository<Role> repository) : IRolesRepository
{
    public async Task AddRoleAsync(Role role, CancellationToken cancellationToken)
    {
        logger.LogInformation("Entering AddRoleAsync");
        await repository.AddAsync(role, cancellationToken);
        logger.LogInformation("Exiting AddRoleAsync");
    }

    public async Task DeleteRoleAsync(RoleId id, CancellationToken cancellationToken)
    {
        logger.LogInformation("Entering DeleteRoleAsync with id: {Id}", id);
        var role = await repository.GetByIdAsync(id, cancellationToken);
        if (role != null)
        {
            await repository.DeleteAsync(role, cancellationToken);
        }
        logger.LogInformation("Exiting DeleteRoleAsync");
    }

    public async Task<IEnumerable<Role>> GetAllRolesAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Entering GetAllRolesAsync");
        var result = await repository.GetAllAsync(cancellationToken);
        logger.LogInformation("Exiting GetAllRolesAsync");
        return result;
    }

    public async Task<Role?> GetRoleByIdAsync(RoleId id, CancellationToken cancellationToken)
    {
        logger.LogInformation("Entering GetRoleByIdAsync with id: {Id}", id);
        var result = await repository.GetByIdAsync(id, cancellationToken);
        logger.LogInformation("Exiting GetRoleByIdAsync");
        return result;
    }

    public async Task UpdateRoleAsync(Role role, CancellationToken cancellationToken)
    {
        logger.LogInformation("Entering UpdateRoleAsync");
        await repository.UpdateAsync(role, cancellationToken);
        logger.LogInformation("Exiting UpdateRoleAsync");
    }

    public async Task<RoleClaim?> GetRoleClaimByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        logger.LogInformation("Entering GetRoleClaimByIdAsync with id: {Id}", id);
        // Implementation for fetching a RoleClaim by ID
        logger.LogInformation("Exiting GetRoleClaimByIdAsync");
        return null; // Replace with actual implementation
    }

    public async Task<IEnumerable<RoleClaim>> GetRoleClaimsByRoleIdAsync(RoleId roleId, CancellationToken cancellationToken)
    {
        logger.LogInformation("Entering GetRoleClaimsByRoleIdAsync with roleId: {RoleId}", roleId);
        // Implementation for fetching RoleClaims by RoleId
        logger.LogInformation("Exiting GetRoleClaimsByRoleIdAsync");
        return Enumerable.Empty<RoleClaim>(); // Replace with actual implementation
    }

    public async Task AddRoleClaimAsync(RoleClaim roleClaim, CancellationToken cancellationToken)
    {
        logger.LogInformation("Entering AddRoleClaimAsync");
        // Implementation for adding a RoleClaim
        logger.LogInformation("Exiting AddRoleClaimAsync");
    }

    public async Task DeleteRoleClaimAsync(Guid id, CancellationToken cancellationToken)
    {
        logger.LogInformation("Entering DeleteRoleClaimAsync with id: {Id}", id);
        // Implementation for deleting a RoleClaim
        logger.LogInformation("Exiting DeleteRoleClaimAsync");
    }

    public async Task UpdateRoleClaimAsync(RoleClaim roleClaim, CancellationToken cancellationToken)
    {
        logger.LogInformation("Entering UpdateRoleClaimAsync");
        // Implementation for updating a RoleClaim
        logger.LogInformation("Exiting UpdateRoleClaimAsync");
    }

    public async Task<Role?> GetRoleByNameAsync(string roleName, CancellationToken cancellationToken)
    {
        logger.LogInformation("Entering GetRoleByNameAsync with roleName: {RoleName}", roleName);
        // Implementation for fetching a Role by Name
        var role = await repository.FindSingleAsync(r => r.Name.Value == roleName, cancellationToken);
        logger.LogInformation("Exiting GetRoleByNameAsync");
        return role;
    }
}