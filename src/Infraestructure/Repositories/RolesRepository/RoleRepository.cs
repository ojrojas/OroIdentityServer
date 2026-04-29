// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.OroIdentityServer.Infraestructure.Repositories;

public class RoleRepository(
    ILogger<RoleRepository> logger,
    IRepository<Role> repository,
    OroIdentityAppContext context) : IRoleRepository
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
}