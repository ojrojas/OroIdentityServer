// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.OroIdentityServer.Infraestructure.Repositories.UserRepository;

public class UserRepository(
    ILogger<UserRepository> logger, IRepository<User> repository) : IUserRepository
{
    public async Task AddUserAsync(User user, CancellationToken cancellationToken)
    {
        logger.LogInformation("Entering AddUserAsync");
        await repository.AddAsync(user, cancellationToken);
        logger.LogInformation("Exiting AddUserAsync");
    }

    public async Task DeleteUserAsync(Guid id, CancellationToken cancellationToken)
    {
        logger.LogInformation("Entering DeleteUserAsync with id: {Id}", id);
        var user = await repository.GetByIdAsync(id);
        if (user != null)
        {
            await repository.DeleteAsync(user, cancellationToken);
        }
        logger.LogInformation("Exiting DeleteUserAsync");
    }

    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        logger.LogInformation("Entering GetAllUsersAsync");
        var result = await repository.GetAllAsync();
        logger.LogInformation("Exiting GetAllUsersAsync");
        return result;
    }

    public async Task<User?> GetUserByIdAsync(Guid id)
    {
        logger.LogInformation("Entering GetUserByIdAsync with id: {Id}", id);
        var result = await repository.GetByIdAsync(id);
        logger.LogInformation("Exiting GetUserByIdAsync");
        return result;
    }

    public async Task UpdateUserAsync(User user, CancellationToken cancellationToken)
    {
        logger.LogInformation("Entering UpdateUserAsync");
        await repository.UpdateAsync(user, cancellationToken);
        logger.LogInformation("Exiting UpdateUserAsync");
    }
}