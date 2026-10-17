// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Application.Modules.Mfa.Commands;

/// <summary>Outcome of a code submission. Only <see cref="UserId"/> on success is usable.</summary>
public sealed record MfaChallengeVerification(
    bool Succeeded,
    Guid UserId,
    bool IsLocked,
    bool UsedRecoveryCode,
    bool RequiresEnrollment,
    int AttemptsRemaining,
    string? ReturnUrl,
    int FailureReason);

public static class MfaFailureReason
{
    public const int None = 0;
    public const int UnknownChallenge = 1;
    public const int ChallengeExpired = 2;
    public const int ChallengeLocked = 3;
    public const int InvalidCode = 4;
    public const int NotEnrolled = 5;
}