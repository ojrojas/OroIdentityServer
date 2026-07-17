// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Core.Modules.Users.Repositories;

/// <summary>
/// Represents a repository interface for managing user entities.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Asynchronously adds a new user to the repository.
    /// </summary>
    /// <param name="user">The user entity to add.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task AddUserAsync(User user, CancellationToken cancellationToken);

    /// <summary>
    /// Asynchronously updates an existing user in the repository.
    /// </summary>
    /// <param name="user">The user entity with updated information.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task UpdateUserAsync(User user, CancellationToken cancellationToken);

    /// <summary>
    /// Asynchronously deletes a user from the repository by their unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the user to delete.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task DeleteUserAsync(UserId id, CancellationToken cancellationToken);

    /// <summary>
    /// Asynchronously retrieves a user from the repository by their unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the user to retrieve.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the user entity if found; otherwise, null.</returns>
    Task<User?> GetUserByIdAsync(UserId id, CancellationToken cancellationToken);

    /// <summary>
    /// Asynchronously retrieves all users from the repository.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains a collection of all user entities.</returns>
    Task<IEnumerable<User>> GetAllUsersAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Gets a user by their email address.
    /// </summary>
    Task<User> GetUserByEmailAsync(string email, CancellationToken cancellationToken);

    /// <summary>
    /// Gets a user by their username or email (login identifier).
    /// </summary>
    Task<User> GetUserByLoginIdentifierAsync(string loginIdentifier, CancellationToken cancellationToken);

    /// <summary>
    /// Change password user
    /// </summary>
    /// <param name="email">Email to find user</param>
    /// <param name="currentPassword">Current password</param>
    /// <param name="newPassword">New password</param>
    /// <param name="confirmedPassword">Confirmed password</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests</param>
    /// <returns>True if password has changed</returns>
    Task<bool> ChangePasswordAsync(string email, string currentPassword, string newPassword, string confirmedPassword, CancellationToken cancellationToken);

    /// <summary>
    /// Validates whether a user with the specified login identifier is allowed to log in.
    /// </summary>
    /// <param name="loginIdentifier">The username or email of the user to validate.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is <c>true</c> if the user can log in; otherwise, <c>false</c>.</returns>
    Task<bool> ValidateUserCanLoginAsync(string loginIdentifier, CancellationToken cancellationToken);
}