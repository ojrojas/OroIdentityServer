// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Infraestructure.Repositories;

public class PermissionRepository(
    ILogger<PermissionRepository> logger,
    IRepository<Permission> repository,
    OroIdentityAppContext context) : IPermissionRepository
{
    public async Task AddPermissionAsync(Permission permission, CancellationToken cancellationToken)
    {
        logger.LogInformation("Entering AddPermissionAsync");
        await repository.AddAsync(permission, cancellationToken);
        logger.LogInformation("Exiting AddPermissionAsync");
    }

    public async Task UpdatePermissionAsync(Permission permission, CancellationToken cancellationToken)
    {
        logger.LogInformation("Entering UpdatePermissionAsync");
        await repository.UpdateAsync(permission, cancellationToken);
        logger.LogInformation("Exiting UpdatePermissionAsync");
    }

    public async Task DeletePermissionAsync(PermissionId id, CancellationToken cancellationToken)
    {
        logger.LogInformation("Entering DeletePermissionAsync with id: {Id}", id);
        var permission = await repository.GetByIdAsync(id, cancellationToken);
        if (permission != null)
        {
            await repository.DeleteAsync(permission, cancellationToken);
        }
        logger.LogInformation("Exiting DeletePermissionAsync");
    }

    public async Task<Permission?> GetPermissionByIdAsync(PermissionId id, CancellationToken cancellationToken)
    {
        logger.LogInformation("Entering GetPermissionByIdAsync with id: {Id}", id);
        var result = await repository.GetByIdAsync(id, cancellationToken);
        logger.LogInformation("Exiting GetPermissionByIdAsync");
        return result;
    }

    public async Task<IEnumerable<Permission>> GetAllPermissionsAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Entering GetAllPermissionsAsync");
        var result = await repository.GetAllAsync(cancellationToken);
        logger.LogInformation("Exiting GetAllPermissionsAsync");
        return result;
    }

    public async Task<IReadOnlyCollection<string>> GetPermissionNamesByUserIdAsync(UserId userId, CancellationToken cancellationToken)
    {
        logger.LogInformation("Entering GetPermissionNamesByUserIdAsync for userId: {UserId}", userId.Value);

        // Roles carry a global query filter (IsActive), so deactivated roles are excluded here.
        var roleIds = await context.UserRoles
            .Where(ur => ur.UserId == userId)
            .Select(ur => ur.RoleId!)
            .ToListAsync(cancellationToken);

        if (roleIds.Count == 0)
        {
            logger.LogInformation("Exiting GetPermissionNamesByUserIdAsync: user has no roles");
            return [];
        }

        var permissionIds = await context.Roles
            .Where(r => roleIds.Contains(r.Id))
            .SelectMany(r => r.RolePermissions.Select(rp => rp.PermissionId))
            .Distinct()
            .ToListAsync(cancellationToken);

        if (permissionIds.Count == 0)
        {
            logger.LogInformation("Exiting GetPermissionNamesByUserIdAsync: roles have no permissions");
            return [];
        }

        var names = await context.Permissions
            .Where(p => permissionIds.Contains(p.Id))
            .Select(p => p.Name)
            .Distinct()
            .OrderBy(name => name)
            .ToListAsync(cancellationToken);

        logger.LogInformation("Exiting GetPermissionNamesByUserIdAsync with {Count} permission(s)", names.Count);
        return names;
    }
}
