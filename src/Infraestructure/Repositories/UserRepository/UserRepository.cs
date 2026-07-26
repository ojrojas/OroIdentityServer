// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Infraestructure.Repositories;

public class UserRepository(
    ILogger<UserRepository> logger,
    IRepository<User> repository,
    ISecurityUserRepository securityUserRepository,
    OroIdentityAppContext context) : IUserRepository
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
        // The generic IRepository<T>.GetAllAsync never applies .Include(), so it queried
        // through OroIdentityAppContext directly here - otherwise User.Roles always comes
        // back empty (no lazy-loading proxies are configured).
        var result = await context.Users.Include(u => u.Roles).ToListAsync(cancellationToken);
        logger.LogInformation("Exiting GetAllUsersAsync");
        return result;
    }

    public async Task<User> GetUserByEmailAsync(string email, CancellationToken cancellationToken)
    {
        logger.LogInformation("handling request user by email {Email}", email);
        var emailSpecification = new GetUserByEmailSpecification(email);
        var user = await repository.FindSingleAsync(emailSpecification.Criteria, cancellationToken);
        logger.LogInformation("finish request get user by email");
        return user ?? throw new InvalidOperationException("User cannot be null.");
    }

    public async Task<User> GetUserByLoginIdentifierAsync(string loginIdentifier, CancellationToken cancellationToken)
    {
        logger.LogInformation("handling request user by login identifier {LoginIdentifier}", loginIdentifier);
        var specification = new GetUserByUserNameOrEmailSpecification(loginIdentifier);
        var user = await repository.FindSingleAsync(specification.Criteria, cancellationToken);
        logger.LogInformation("finish request get user by login identifier");
        return user ?? throw new InvalidOperationException("User cannot be null.");
    }

    public async Task<User?> GetUserByIdAsync(UserId id, CancellationToken cancellationToken)
    {
        logger.LogInformation("Entering GetUserByIdAsync with id: {Id}", id);
        var specification = new GetUserByIdSpecification(id);

        // specification.Includes existed but was never consumed by anything - the generic
        // IRepository<T>.FindSingleAsync it used to go through has no .Include() support at
        // all, so User.Roles always came back empty here. Query the DbContext directly and
        // eager-load Roles explicitly instead.
        var user = await context.Users
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(specification.Criteria, cancellationToken);

        logger.LogInformation("Exiting GetUserByIdAsync");
        return user;
    }

    public async Task UpdateUserAsync(User user, CancellationToken cancellationToken)
    {
        logger.LogInformation("Entering UpdateUserAsync");
        await repository.UpdateAsync(user, cancellationToken);
        logger.LogInformation("Exiting UpdateUserAsync");
    }

    public async Task<bool> ValidateUserCanLoginAsync(string loginIdentifier, CancellationToken cancellationToken)
    {
        logger.LogInformation("Validating if user can login with identifier: {LoginIdentifier}", loginIdentifier);
        var user = await GetUserByLoginIdentifierAsync(loginIdentifier, cancellationToken);

        if (user is null)
        {
            logger.LogWarning("User not found with identifier: {LoginIdentifier}", loginIdentifier);
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
