// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Core.Modules.Mfa.Repositories;

using OroIdentityServer.Core.Modules.Mfa.Aggregates;
using OroIdentityServer.Core.Modules.Users.ValueObjects;

public interface IMfaLoginChallengeRepository
{
    Task AddAsync(MfaLoginChallenge challenge, CancellationToken cancellationToken = default);
    Task UpdateAsync(MfaLoginChallenge challenge, CancellationToken cancellationToken = default);
    Task DeleteAsync(MfaLoginChallenge challenge, CancellationToken cancellationToken = default);

    Task<MfaLoginChallenge?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);

    /// <summary>Open challenges for a user, used to supersede stale ones on a new login attempt.</summary>
    Task<IEnumerable<MfaLoginChallenge>> GetOpenByUserIdAsync(UserId userId, CancellationToken cancellationToken = default);
}