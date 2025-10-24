// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.OroIdentityServer.Infraestructure.Repositories.Interfaces;

public interface IUserRepository 
{
    Task AddUserAsync(User user, CancellationToken cancellationToken);
    Task UpdateUserAsync(User user, CancellationToken cancellationToken);
    Task DeleteUserAsync(Guid id, CancellationToken cancellationToken);
    Task<User?> GetUserByIdAsync(Guid id);
    Task<IEnumerable<User>> GetAllUsersAsync();
}