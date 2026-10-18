// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Core.Modules.Mfa;

/// <summary>
/// Brute-force policy for the TOTP second factor.
/// </summary>
/// <remarks>
/// Two budgets, because they defend against two different things. The per-challenge budget
/// (<see cref="MaxAttemptsPerChallenge"/>) stops a single login from being ground down: it is
/// enforced by <see cref="Aggregates.MfaLoginChallenge"/>. The per-credential budget
/// (<see cref="MaxAttemptsPerWindow"/> over <see cref="AttemptWindow"/>) is the one that actually
/// bounds the search space, because the challenge counter resets the moment the user retypes the
/// password and gets a fresh challenge - without it, a 6 digit code (~10^6 values) would fall to
/// an offline-feel brute force at one login per second. Once spent, the credential is unusable
/// for <see cref="LockoutDuration"/>, so guessing is bounded in time as well as in attempts.
/// </remarks>
public sealed class MfaLockoutPolicy
{
    public const string SectionName = "Mfa";

    /// <summary>Wrong codes a single login challenge tolerates before it dies.</summary>
    public int MaxAttemptsPerChallenge => 5;

    /// <summary>Wrong codes tolerated per credential inside <see cref="AttemptWindow"/>.</summary>
    public int MaxAttemptsPerWindow { get; set; } = 5;

    /// <summary>Sliding window over which the failed codes are counted.</summary>
    public TimeSpan AttemptWindow { get; set; } = TimeSpan.FromMinutes(15);

    /// <summary>How long the credential refuses codes once the window budget is spent.</summary>
    public TimeSpan LockoutDuration { get; set; } = TimeSpan.FromMinutes(15);

    /// <summary>How long the current lockout still has to run, or <see cref="TimeSpan.Zero"/>.</summary>
    public TimeSpan RemainingLockout(DateTime nowUtc, DateTime? lockedUntilUtc)
    {
        if (lockedUntilUtc is not { } until || until <= nowUtc)
        {
            return TimeSpan.Zero;
        }

        return until - nowUtc;
    }

    public MfaLockoutPolicy Normalize()
    {
        if (MaxAttemptsPerWindow < 1) MaxAttemptsPerWindow = 1;
        if (AttemptWindow <= TimeSpan.Zero) AttemptWindow = TimeSpan.FromMinutes(15);
        if (LockoutDuration <= TimeSpan.Zero) LockoutDuration = TimeSpan.FromMinutes(15);
        return this;
    }
}