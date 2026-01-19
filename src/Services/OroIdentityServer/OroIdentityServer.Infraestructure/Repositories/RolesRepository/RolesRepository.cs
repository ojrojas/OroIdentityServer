// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.OroIdentityServer.Infraestructure.Repositories;

public class RolesRepository(
    ILogger<RolesRepository> logger, 
    IRepository<Role> repository,
    OroIdentityAppContext context) : IRolesRepository
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
        var roleClaim = await context.Roles
            .SelectMany(r => r.Claims)
            .FirstOrDefaultAsync(rc => rc.Id == id, cancellationToken);
        logger.LogInformation("Exiting GetRoleClaimByIdAsync");
        return roleClaim;
    }

    public async Task<IEnumerable<RoleClaim>> GetRoleClaimsByRoleIdAsync(RoleId roleId, CancellationToken cancellationToken)
    {
        logger.LogInformation("Entering GetRoleClaimsByRoleIdAsync with roleId: {RoleId}", roleId);
        var role = await repository.GetByIdAsync(roleId, cancellationToken);
        var result = role?.Claims ?? Enumerable.Empty<RoleClaim>();
        logger.LogInformation("Exiting GetRoleClaimsByRoleIdAsync");
        return result;
    }

    public async Task AddRoleClaimAsync(RoleId roleId, RoleClaimType claimType, RoleClaimValue claimValue, CancellationToken cancellationToken)
    {
        logger.LogInformation("Entering AddRoleClaimAsync for roleId: {RoleId}", roleId);
        var role = await repository.GetByIdAsync(roleId, cancellationToken);
        if (role != null)
        {
            var newClaim = new RoleClaim(claimType, claimValue);
            role.AddClaim(newClaim);
            await repository.UpdateAsync(role, cancellationToken);
        }
        logger.LogInformation("Exiting AddRoleClaimAsync");
    }

    public async Task DeleteRoleClaimAsync(Guid id, CancellationToken cancellationToken)
    {
        logger.LogInformation("Entering DeleteRoleClaimAsync with id: {Id}", id);
        var role = await context.Roles.FirstOrDefaultAsync(r => r.Claims.Any(c => c.Id == id), cancellationToken);
        if (role != null)
        {
            var claim = role.Claims.FirstOrDefault(c => c.Id == id);
            if (claim != null)
            {
                role.RemoveClaim(claim);
                await repository.UpdateAsync(role, cancellationToken);
            }
        }
        logger.LogInformation("Exiting DeleteRoleClaimAsync");
    }

    public async Task UpdateRoleClaimAsync(Guid claimId, RoleClaimType newClaimType, RoleClaimValue newClaimValue, CancellationToken cancellationToken)
    {
        logger.LogInformation("Entering UpdateRoleClaimAsync with claimId: {ClaimId}", claimId);
        var role = await context.Roles.FirstOrDefaultAsync(r => r.Claims.Any(c => c.Id == claimId), cancellationToken);
        if (role != null)
        {
            var existingClaim = role.Claims.FirstOrDefault(c => c.Id == claimId);
            if (existingClaim != null)
            {
                role.RemoveClaim(existingClaim);
                var newClaim = new RoleClaim(newClaimType, newClaimValue);
                role.AddClaim(newClaim);
                await repository.UpdateAsync(role, cancellationToken);
            }
        }
        logger.LogInformation("Exiting UpdateRoleClaimAsync");
    }

    public async Task<Role?> GetRoleByNameAsync(string roleName, CancellationToken cancellationToken)
    {
        logger.LogInformation("Entering GetRoleByNameAsync with roleName: {RoleName}", roleName);
        var role = await repository.FindSingleAsync(r => r.Name != null && r.Name.Value == roleName, cancellationToken);
        logger.LogInformation("Exiting GetRoleByNameAsync");
        return role;
    }
}