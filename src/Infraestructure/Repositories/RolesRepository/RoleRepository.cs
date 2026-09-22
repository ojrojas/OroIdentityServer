// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Infraestructure.Repositories;

public class RoleRepository(
    ILogger<RoleRepository> logger,
    IRepository<Role> repository,
    IUserRolesRepository userRolesRepository,
    OroIdentityAppContext? context = null) : IRoleRepository
{
    public async Task AddAsync(Role role, CancellationToken cancellationToken)
    {
        logger.LogInformation("Entering AddAsync");
        await repository.AddAsync(role, cancellationToken);
        logger.LogInformation("Exiting AddAsync");
    }

    public async Task DeleteAsync(RoleId id, CancellationToken cancellationToken)
    {
        logger.LogInformation("Entering DeleteAsync with id: {Id}", id);
        var role = await repository.GetByIdAsync(id, cancellationToken);
        if (role != null)
        {
            await repository.DeleteAsync(role, cancellationToken);
        }
        logger.LogInformation("Exiting DeleteAsync");
    }

    public async Task<IEnumerable<Role>> GetAllAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Entering GetAllAsync");
        var result = await repository.GetAllAsync(cancellationToken);
        logger.LogInformation("Exiting GetAllAsync");
        return result;
    }

    public async Task<Role?> GetByIdAsync(RoleId id, CancellationToken cancellationToken)
    {
        logger.LogInformation("Entering GetByIdAsync with id: {Id}", id);
        var result = await repository.GetByIdAsync(id, cancellationToken);
        logger.LogInformation("Exiting GetByIdAsync");
        return result;
    }

    public async Task<Role?> GetByIdIgnoringFiltersAsync(RoleId id, CancellationToken cancellationToken)
    {
        logger.LogInformation("Entering GetByIdIgnoringFiltersAsync with id: {Id}", id);
        var specification = new GetRoleByIdIgnoringFiltersSpecification(id);
        var result = await repository.FirstOrDefaultAsync(specification, cancellationToken);
        logger.LogInformation("Exiting GetByIdIgnoringFiltersAsync");
        return result;
    }

    public async Task UpdateAsync(Role role, CancellationToken cancellationToken)
    {
        logger.LogInformation("Entering UpdateAsync for roleId: {RoleId}", role.Id);
        await repository.UpdateAsync(role, cancellationToken);
        logger.LogInformation("Exiting UpdateAsync for roleId: {RoleId}", role.Id);
    }

    public async Task<Role?> GetRoleByNameAsync(RoleName roleName, CancellationToken cancellationToken)
    {
        logger.LogInformation("Entering GetRoleByNameAsync with roleName: {RoleName}", roleName);
        var role = await repository.FindSingleAsync(r => r.Name != null && r.Name == roleName, cancellationToken);
        logger.LogInformation("Exiting GetRoleByNameAsync");
        return role;
    }

    public async Task<IEnumerable<Role>> GetRolesByUserIdAsync(UserId userId, CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting roles by user id with value: {userId}", userId);
        var userRoles = await userRolesRepository.GetRolesByUserIdAsync(userId, cancellationToken);
        var roleIds = userRoles
            .Select(ur => ur.RoleId)
            .Where(id => id is not null)
            .Select(id => id!)
            .ToList();

        if (roleIds.Count == 0)
            return [];

        var roles = await repository.ListAsync(new GetRolesByUserIdSpecification(roleIds), cancellationToken);
        logger.LogInformation("Exiting GetRolesByUserIdAsync");
        return roles;
    }

    public async Task<int> CountCreatedTodayAsync(DateTime today, CancellationToken cancellationToken)
    {
        logger.LogInformation("Counting roles created since {Today}", today);
        return await repository.CountAsync(new GetRolesCreatedTodaySpecification(today), cancellationToken);
    }

    public async Task<bool> HasPermissionsAsync(RoleId roleId, CancellationToken cancellationToken)
    {
        logger.LogInformation("Checking permissions for role: {RoleId}", roleId);
        var role = await repository.FirstOrDefaultAsync(new GetRoleWithPermissionsSpecification(roleId), cancellationToken);
        return role?.RolePermissions.Count > 0;
    }

    public async Task<Role?> GetWithPermissionsAsync(RoleId roleId, CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting role with permissions for role: {RoleId}", roleId);

        // The context is globally configured with QueryTrackingBehavior.NoTracking (see
        // InfraestructureExtensions), so an update path must opt back into tracking. Otherwise
        // Repository.UpdateAsync receives a detached graph and marks keyed RolePermission
        // children as Modified instead of Added, so new assignments are never inserted.
        if (context is not null)
        {
            return await context.Roles
                .AsTracking()
                .Include(r => r.RolePermissions)
                .FirstOrDefaultAsync(r => r.Id == roleId, cancellationToken);
        }

        return await repository.FirstOrDefaultAsync(new GetRoleWithPermissionsSpecification(roleId), cancellationToken);
    }

    public async Task<Role?> GetWithPermissionsNoTrackingAsync(RoleId roleId, CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting role with permissions (no tracking) for role: {RoleId}", roleId);

        // The context is globally NoTracking, so the specification-based read is already
        // detached; this is the read path used by queries (never mutates the graph).
        return await repository.FirstOrDefaultAsync(new GetRoleWithPermissionsSpecification(roleId), cancellationToken);
    }
}