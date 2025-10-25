// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.OroIdentityServer.Infraestructure.Repositories;

public class RolesRepository(
    ILogger<RolesRepository> logger, IRepository<Role> repository) : IRolesRepository
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
}