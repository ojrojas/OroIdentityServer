// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Core.Modules.Mfa.Repositories;

using OroIdentityServer.Core.Modules.Mfa.Aggregates;
using OroIdentityServer.Core.Modules.Users.ValueObjects;

public interface IMfaRecoveryCodeRepository
{
    Task AddAsync(MfaRecoveryCode code, CancellationToken cancellationToken = default);
    Task UpdateAsync(MfaRecoveryCode code, CancellationToken cancellationToken = default);
    Task DeleteAsync(MfaRecoveryCode code, CancellationToken cancellationToken = default);

    /// <summary>
    /// Unused codes of a credential. The candidate submitted by the user is verified against
    /// each hash (BCrypt is salted, so it cannot be looked up by hash).
    /// </summary>
    Task<IEnumerable<MfaRecoveryCode>> GetUnusedByUserIdAsync(UserId userId, CancellationToken cancellationToken = default);

    Task<IEnumerable<MfaRecoveryCode>> GetByCredentialIdAsync(MfaCredentialId credentialId, CancellationToken cancellationToken = default);
}