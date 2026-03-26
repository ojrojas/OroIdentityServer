// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.

namespace OroIdentityServer.OroIdentityServer.Infraestructure.Repositories;

public class UserRolesRepository(
    ILogger<UserRolesRepository> logger,
    IRepository<UserRole> repository) : IUserRolesRepository
{
    public async Task<IEnumerable<UserRole>> GetAllRolesAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Entering GetAllRolesAsync");
        var roles = await repository.GetAllAsync(cancellationToken);
        logger.LogInformation("Exiting GetAllRolesAsync");
        return roles;
    }

    public async Task AddUserRoleAsync(UserRole userRole, CancellationToken cancellationToken)
    {
        logger.LogInformation("Entering AddUserRoleAsync");
        await repository.AddAsync(userRole, cancellationToken);
        logger.LogInformation("Exiting AddUserRoleAsync");
    }

    public async Task DeleteUserRoleAsync(UserRole userRole, CancellationToken cancellationToken)
    {
        logger.LogInformation("Entering DeleteUserRoleAsync");
        await repository.DeleteAsync(userRole, cancellationToken);
        logger.LogInformation("Exiting DeleteUserRoleAsync");
    }

    public async Task<IEnumerable<UserRole>> GetRolesByUserIdAsync(UserId userId, CancellationToken cancellationToken)
    {
        logger.LogInformation("Entering GetRolesByUserIdAsync");
        var specification = new GetUserRolesByUserIdSpecification(userId);
        var roles = await repository.CurrentContext.Where(
            specification.Criteria).ToListAsync(cancellationToken);
        logger.LogInformation("Exiting GetRolesByUserIdAsync");
        return roles;
    }

    public async Task DeleteRolesByUserIdAsync(UserId userId, CancellationToken cancellationToken)
    {
        logger.LogInformation("Entering DeleteRolesByUserIdAsync");
        var roles = await GetRolesByUserIdAsync(userId, cancellationToken);
        foreach (var role in roles)
        {
            await repository.DeleteAsync(role, cancellationToken);
        }
        logger.LogInformation("Exiting DeleteRolesByUserIdAsync");
    }
}
