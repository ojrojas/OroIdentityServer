// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Core.Modules.Mfa.Aggregates;

using OroIdentityServer.Core.Modules.Mfa.ValueObjects;
using OroIdentityServer.Core.Modules.Users.ValueObjects;

/// <summary>
/// A single-use recovery code: the fallback for a user who lost their phone. Only the hash is
/// stored, so the plaintext is shown once at generation time.
/// </summary>
public class MfaRecoveryCode : AggregateRoot<MfaRecoveryCodeId>
{
    private MfaRecoveryCode()
    {
        MfaCredentialId = null!;
        UserId = null!;
        CodeHash = string.Empty;
    }

    public MfaRecoveryCode(MfaCredentialId credentialId, UserId userId, string codeHash)
    {
        Id = MfaRecoveryCodeId.New();
        MfaCredentialId = credentialId;
        UserId = userId;
        CodeHash = codeHash;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public MfaCredentialId MfaCredentialId { get; private set; }
    public UserId UserId { get; private set; }
    public string CodeHash { get; private set; } = string.Empty;
    public bool IsUsed { get; private set; }
    public DateTime? UsedAtUtc { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    public void MarkUsed(DateTime nowUtc)
    {
        IsUsed = true;
        UsedAtUtc = nowUtc;
    }
}