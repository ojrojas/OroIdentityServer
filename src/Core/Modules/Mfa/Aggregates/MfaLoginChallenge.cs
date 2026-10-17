// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Core.Modules.Mfa.Aggregates;

using OroIdentityServer.Core.Modules.Mfa.Enums;
using OroIdentityServer.Core.Modules.Mfa.ValueObjects;
using OroIdentityServer.Core.Modules.Users.ValueObjects;

/// <summary>
/// The server-side half of the "second factor pending" state. It exists in the database (and
/// not only in the short-lived pending cookie) so the attempt budget cannot be replayed by a
/// client holding an old copy of the cookie: a six digit TOTP code has only ~10^6 possible
/// values, so the number of tries per challenge must be capped on the server.
/// </summary>
public class MfaLoginChallenge : AggregateRoot<MfaLoginChallengeId>
{
    /// <summary>How many wrong codes a single challenge tolerates before it is locked.</summary>
    public const int MaxAttempts = 5;

    private MfaLoginChallenge()
    {
        UserId = null!;
        Token = string.Empty;
    }

    public MfaLoginChallenge(
        UserId userId,
        MfaChallengePurpose purpose,
        DateTime expiresAtUtc,
        string? returnUrl = null,
        string? ipAddress = null,
        string? userAgent = null)
    {
        Id = MfaLoginChallengeId.New();
        Token = Guid.NewGuid().ToString("N");
        UserId = userId;
        Purpose = purpose;
        Status = MfaChallengeStatus.Pending;
        CreatedAtUtc = DateTime.UtcNow;
        ExpiresAtUtc = expiresAtUtc;
        ReturnUrl = returnUrl;
        IpAddress = ipAddress;
        UserAgent = userAgent;
        RaiseDomainEvent(new MfaChallengeCreatedEvent(Id, userId, purpose, expiresAtUtc));
    }

    public UserId UserId { get; private set; }

    /// <summary>Opaque, single-use handle handed to the browser in the pending cookie.</summary>
    public string Token { get; private set; } = string.Empty;

    public MfaChallengePurpose Purpose { get; private set; }
    public MfaChallengeStatus Status { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime ExpiresAtUtc { get; private set; }
    public DateTime? CompletedAtUtc { get; private set; }
    public string? ReturnUrl { get; private set; }
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }
    public int Attempts { get; private set; }

    public bool IsExpired(DateTime nowUtc) => ExpiresAtUtc <= nowUtc;

    /// <summary>True only while the challenge can still accept a code right now.</summary>
    public bool CanBeUsed(DateTime nowUtc) =>
        Status is MfaChallengeStatus.Pending or MfaChallengeStatus.Failed && !IsExpired(nowUtc);

    /// <summary>
    /// Counts a wrong code. Once the budget is spent the challenge is locked, which forces the
    /// user back through the password step to get a fresh (and newly counted) challenge.
    /// </summary>
    public bool RegisterFailedAttempt(DateTime nowUtc)
    {
        if (Status is MfaChallengeStatus.Completed or MfaChallengeStatus.Locked or MfaChallengeStatus.Superseded)
            return false;

        if (IsExpired(nowUtc))
        {
            Status = MfaChallengeStatus.Locked;
            return false;
        }

        Attempts++;

        if (Attempts >= MaxAttempts)
        {
            Status = MfaChallengeStatus.Locked;
            RaiseDomainEvent(new MfaChallengeLockedEvent(Id, UserId, Attempts));
            return false;
        }

        Status = MfaChallengeStatus.Failed;
        RaiseDomainEvent(new MfaChallengeFailedEvent(Id, UserId, Attempts));
        return true;
    }

    public void Complete(DateTime nowUtc)
    {
        Status = MfaChallengeStatus.Completed;
        CompletedAtUtc = nowUtc;
        RaiseDomainEvent(new MfaChallengeCompletedEvent(Id, UserId, nowUtc));
    }

    /// <summary>Kills an older challenge when the user starts a new login attempt.</summary>
    public void Supersede()
    {
        if (Status is MfaChallengeStatus.Completed) return;
        Status = MfaChallengeStatus.Superseded;
    }
}