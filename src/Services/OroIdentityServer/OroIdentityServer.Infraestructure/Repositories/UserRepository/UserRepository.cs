// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.OroIdentityServer.Infraestructure.Repositories;

public class UserRepository(
    ILogger<UserRepository> logger, 
    IRepository<User> repository,
    IUserRolesRepository userRolesRepository,
    ISecurityUserRepository securityUserRepository) : IUserRepository
{
    public async Task AddUserAsync(User user, CancellationToken cancellationToken)
    {
        logger.LogInformation("Entering AddUserAsync");
        await repository.AddAsync(user, cancellationToken);
        logger.LogInformation("Exiting AddUserAsync");
    }

    public async Task<bool> ChangePasswordAsync(string email, string currentPassword, string newPassword, string confirmedPassword, CancellationToken cancellationToken)
    {
        logger.LogInformation("Entering ChangePasswordAsync for email: {Email}", email);

        if (newPassword != confirmedPassword)
        {
            logger.LogWarning("New password and confirmed password do not match for email: {Email}", email);
            return false;
        }

        var user = await GetUserByEmailAsync(email, cancellationToken);
        if (user == null)
        {
            logger.LogWarning("User not found with email: {Email}", email);
            return false;
        }

        if (user?.SecurityUser?.PasswordHash == null || !user.SecurityUser.PasswordHash.Equals(currentPassword))
        {
            logger.LogWarning("Current password is incorrect or user is null for email: {Email}", email);
            return false;
        }

        // user.SecurityUser.PasswordHash = newPassword;
        await repository.UpdateAsync(user, cancellationToken);

        logger.LogInformation("Password successfully changed for email: {Email}", email);
        return true;
    }

    public async Task DeleteUserAsync(UserId id, CancellationToken cancellationToken)
    {
        logger.LogInformation("Entering DeleteUserAsync with id: {Id}", id);
        var user = await repository.GetByIdAsync(id,cancellationToken);
        if (user != null)
        {
            await repository.DeleteAsync(user, cancellationToken);
        }
        logger.LogInformation("Exiting DeleteUserAsync");
    }

    public async Task<IEnumerable<User>> GetAllUsersAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Entering GetAllUsersAsync");
        var result = await repository.GetAllAsync(cancellationToken);
        logger.LogInformation("Exiting GetAllUsersAsync");
        return result;
    }

    public async Task<User> GetUserByEmailAsync(string email, CancellationToken cancellationToken)
    {
        logger.LogInformation("handling request user by email {Email}", email);
        var emailSpecification = new GetUserByEmailSpecification(email);
        var user = await repository.CurrentContext.FirstOrDefaultAsync(emailSpecification.Criteria, cancellationToken: cancellationToken);
        logger.LogInformation("finish request get user by email");
        return user ?? throw new InvalidOperationException("User cannot be null.");
    }

    public async Task<User?> GetUserByIdAsync(UserId id, CancellationToken cancellationToken)
    {
        logger.LogInformation("Entering GetUserByIdAsync with id: {Id}", id);
        var specification = new GetUserByIdSpecification(id);
        var user = await repository.CurrentContext.Include(x=> x.Roles).FirstOrDefaultAsync(
            specification.Criteria, cancellationToken: cancellationToken);

        logger.LogInformation("Exiting GetUserByIdAsync");
        return user;
    }

    public async Task UpdateUserAsync(User user, CancellationToken cancellationToken)
    {
        logger.LogInformation("Entering UpdateUserAsync");
        await repository.UpdateAsync(user, cancellationToken);
        logger.LogInformation("Exiting UpdateUserAsync");
    }

    public async Task<bool> ValidateUserCanLoginAsync(string email, CancellationToken cancellationToken)
    {
        logger.LogInformation("Validating if user can login with email: {Email}", email);
        var user = await GetUserByEmailAsync(email, cancellationToken);

        if (user is null)
        {
            logger.LogWarning("User not found with email: {Email}", email);
            return false;
        }

        var securityUser = await securityUserRepository.GetSecurityUserAsync(user.SecurityUserId!.Value, cancellationToken);

        if (securityUser.LockoutEnabled && securityUser.LockoutEnd.HasValue && securityUser.LockoutEnd.Value > DateTime.UtcNow)
        {
            logger.LogWarning("User is locked out until {LockoutEnd}", securityUser.LockoutEnd);
            return false;
        }

        logger.LogInformation("User is allowed to login");
        return true;
    }

}
