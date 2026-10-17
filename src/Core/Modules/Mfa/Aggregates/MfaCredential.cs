// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Core.Modules.Mfa.Aggregates;

using OroIdentityServer.Core.Modules.Mfa.Enums;
using OroIdentityServer.Core.Modules.Mfa.ValueObjects;
using OroIdentityServer.Core.Modules.Users.ValueObjects;

/// <summary>
/// A user's authenticator-app (TOTP) factor. There is at most one credential per user: the
/// secret is stored encrypted at rest with Data Protection and is only ever handed to the
/// user at enrolment time, through the <c>otpauth://</c> provisioning URI.
/// </summary>
public class MfaCredential : AggregateRoot<MfaCredentialId>
{
    private MfaCredential()
    {
        UserId = null!;
        EncryptedSecret = string.Empty;
    }

    public MfaCredential(UserId userId, string encryptedSecret)
    {
        Id = MfaCredentialId.New();
        UserId = userId;
        EncryptedSecret = encryptedSecret;
        IsEnabled = false;
        CreatedAtUtc = DateTime.UtcNow;
        RaiseDomainEvent(new MfaCredentialEnrolledEvent(Id, userId));
    }

    public UserId UserId { get; private set; }
    public string EncryptedSecret { get; private set; } = string.Empty;
    public bool IsEnabled { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? ConfirmedAtUtc { get; private set; }
    public DateTime? LastVerifiedAtUtc { get; private set; }

    /// <summary>
    /// Highest TOTP time step (30 second windows since the Unix epoch) already consumed by
    /// this credential. Keeps a code from being replayed inside its own window, and keeps the
    /// verification window (now +/- 1 step) from accepting an already-spent code.
    /// </summary>
    public long LastUsedTimeStep { get; private set; } = -1;

    /// <summary>
    /// Marks the factor as usable after the first successful code, which also consumes that
    /// time step so the enrolment code cannot be replayed as a login code.
    /// </summary>
    public void Confirm(DateTime nowUtc, long timeStep)
    {
        IsEnabled = true;
        ConfirmedAtUtc = nowUtc;
        LastVerifiedAtUtc = nowUtc;
        LastUsedTimeStep = timeStep;
        RaiseDomainEvent(new MfaCredentialConfirmedEvent(Id, UserId, nowUtc));
    }

    /// <summary>Replaces the secret and drops back to "not enrolled yet" state.</summary>
    public void RestartEnrollment(string encryptedSecret)
    {
        IsEnabled = false;
        EncryptedSecret = encryptedSecret;
        ConfirmedAtUtc = null;
        LastUsedTimeStep = -1;
        RaiseDomainEvent(new MfaCredentialEnrolledEvent(Id, UserId));
    }

    public void Disable()
    {
        IsEnabled = false;
        ConfirmedAtUtc = null;
        LastUsedTimeStep = -1;
        RaiseDomainEvent(new MfaCredentialDisabledEvent(Id, UserId, DateTime.UtcNow));
    }

    /// <summary>Records a successful verification, ignoring steps already spent.</summary>
    public void RecordVerifiedUse(long timeStep, DateTime nowUtc)
    {
        LastUsedTimeStep = Math.Max(LastUsedTimeStep, timeStep);
        LastVerifiedAtUtc = nowUtc;
    }
}