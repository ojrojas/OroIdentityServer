// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.OroIdentityServer.Infraestructure.Interfaces;

public interface IUserSessionRepository
{
    Task AddUserSessionAsync(UserSession session, CancellationToken cancellationToken);
    Task UpdateUserSessionAsync(UserSession session, CancellationToken cancellationToken);
    Task DeleteUserSessionAsync(UserSession session, CancellationToken cancellationToken);
    Task<UserSession?> GetUserSessionByIdAsync(UserSessionId id, CancellationToken cancellationToken);
    Task<IEnumerable<UserSession>> GetSessionsByUserIdAsync(UserId userId, CancellationToken cancellationToken);
    Task<UserSession?> GetByTokenAsync(string sessionToken, CancellationToken cancellationToken);
}
