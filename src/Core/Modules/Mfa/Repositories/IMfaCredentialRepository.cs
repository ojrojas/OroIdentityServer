// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Core.Modules.Mfa.Repositories;

using OroIdentityServer.Core.Modules.Mfa.Aggregates;
using OroIdentityServer.Core.Modules.Users.ValueObjects;

public interface IMfaCredentialRepository
{
    Task AddAsync(MfaCredential credential, CancellationToken cancellationToken = default);
    Task UpdateAsync(MfaCredential credential, CancellationToken cancellationToken = default);
    Task DeleteAsync(MfaCredential credential, CancellationToken cancellationToken = default);

    /// <summary>At most one credential per user; null when the user never started enrolment.</summary>
    Task<MfaCredential?> GetByUserIdAsync(UserId userId, CancellationToken cancellationToken = default);
}