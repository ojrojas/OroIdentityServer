// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Infraestructure.Repositories;

public class UserPermissionsRepository(
    ILogger<UserPermissionsRepository> logger,
    IRepository<UserPermission> repository) : IUserPermissionsRepository
{
    public async Task AddUserPermissionAsync(UserPermission userPermission, CancellationToken cancellationToken)
    {
        logger.LogInformation("Entering AddUserPermissionAsync");
        await repository.AddAsync(userPermission, cancellationToken);
        logger.LogInformation("Exiting AddUserPermissionAsync");
    }

    public async Task DeleteUserPermissionAsync(UserPermission userPermission, CancellationToken cancellationToken)
    {
        logger.LogInformation("Entering DeleteUserPermissionAsync");
        await repository.DeleteAsync(userPermission, cancellationToken);
        logger.LogInformation("Exiting DeleteUserPermissionAsync");
    }

    public async Task<IEnumerable<UserPermission>> GetPermissionsByUserIdAsync(UserId userId, CancellationToken cancellationToken)
    {
        logger.LogInformation("Entering GetPermissionsByUserIdAsync");
        var specification = new GetUserPermissionsByUserIdSpecification(userId);
        var permissions = await repository.ListAsync(specification, cancellationToken);
        logger.LogInformation("Exiting GetPermissionsByUserIdAsync");
        return permissions;
    }

    public async Task DeletePermissionsByUserIdAsync(UserId userId, CancellationToken cancellationToken)
    {
        logger.LogInformation("Entering DeletePermissionsByUserIdAsync");
        var permissions = await GetPermissionsByUserIdAsync(userId, cancellationToken);
        foreach (var permission in permissions)
        {
            await repository.DeleteAsync(permission, cancellationToken);
        }
        logger.LogInformation("Exiting DeletePermissionsByUserIdAsync");
    }
}
