// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.OroIdentityServer.Infraestructure.Repositories;

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
}
