// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.OroIdentityServer.Infraestructure.Repositories;

public class RolesRepository(
    ILogger<RolesRepository> logger, 
    IRepository<Role> repository,
    IRepository<RoleClaim> roleClaimRepository) : IRolesRepository
{
    public async Task AddRoleAsync(Role role, CancellationToken cancellationToken)
    {
        logger.LogInformation("Entering AddRoleAsync");
        await repository.AddAsync(role, cancellationToken);
        logger.LogInformation("Exiting AddRoleAsync");
    }

    public async Task DeleteRoleAsync(Guid id, CancellationToken cancellationToken)
    {
        logger.LogInformation("Entering DeleteRoleAsync with id: {Id}", id);
        var role = await repository.GetByIdAsync(id);
        if (role != null)
        {
            await repository.DeleteAsync(role, cancellationToken);
        }
        logger.LogInformation("Exiting DeleteRoleAsync");
    }

    public async Task<IEnumerable<Role>> GetAllRolesAsync()
    {
        logger.LogInformation("Entering GetAllRolesAsync");
        var result = await repository.GetAllAsync();
        logger.LogInformation("Exiting GetAllRolesAsync");
        return result;
    }

    public async Task<Role?> GetRoleByIdAsync(Guid id)
    {
        logger.LogInformation("Entering GetRoleByIdAsync with id: {Id}", id);
        var result = await repository.GetByIdAsync(id);
        logger.LogInformation("Exiting GetRoleByIdAsync");
        return result;
    }

    public async Task UpdateRoleAsync(Role role, CancellationToken cancellationToken)
    {
        logger.LogInformation("Entering UpdateRoleAsync");
        await repository.UpdateAsync(role, cancellationToken);
        logger.LogInformation("Exiting UpdateRoleAsync");
    }

    public async Task<RoleClaim?> GetRoleClaimByIdAsync(Guid roleClaimId)
    {
        logger.LogInformation("Entering GetRoleClaimByIdAsync with id: {Id}", roleClaimId);
        var result = await roleClaimRepository.GetByIdAsync(roleClaimId);
        logger.LogInformation("Exiting GetRoleClaimByIdAsync");
        return result;
    }

    public async Task UpdateRoleClaimAsync(RoleClaim roleClaim, CancellationToken cancellationToken)
    {
        logger.LogInformation("Entering UpdateRoleClaimAsync");
        await roleClaimRepository.UpdateAsync(roleClaim, cancellationToken);
        logger.LogInformation("Exiting UpdateRoleClaimAsync");
    }

    public async Task AddRoleClaimAsync(RoleClaim roleClaim, CancellationToken cancellationToken)
    {
        logger.LogInformation("Entering AddRoleClaimAsync");
        await roleClaimRepository.AddAsync(roleClaim, cancellationToken);
        logger.LogInformation("Exiting AddRoleClaimAsync");
    }

    public async Task DeleteRoleClaimAsync(RoleClaim roleClaim, CancellationToken cancellationToken)
    {
        logger.LogInformation("Entering DeleteRoleClaimAsync");
        await roleClaimRepository.DeleteAsync(roleClaim, cancellationToken);
        logger.LogInformation("Exiting DeleteRoleClaimAsync");
    }

    public async Task<IEnumerable<RoleClaim>> GetRoleClaimsByRoleIdAsync(Guid roleId)
    {
        logger.LogInformation("Entering GetRoleClaimsByRoleIdAsync with RoleId: {RoleId}", roleId);
        var result = await roleClaimRepository.CurrentContext.Where(rc => rc.RoleId == roleId).ToListAsync();
        logger.LogInformation("Exiting GetRoleClaimsByRoleIdAsync");
        return result;
    }
}