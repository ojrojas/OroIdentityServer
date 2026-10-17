// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Core.Modules.Mfa.DomainEvents;

using OroIdentityServer.Core.Modules.Mfa.Enums;
using OroIdentityServer.Core.Modules.Mfa.ValueObjects;
using OroIdentityServer.Core.Modules.Users.ValueObjects;

public record MfaCredentialEnrolledEvent(MfaCredentialId CredentialId, UserId UserId) : DomainEvent;

public record MfaCredentialConfirmedEvent(MfaCredentialId CredentialId, UserId UserId, DateTime ConfirmedAtUtc) : DomainEvent;

public record MfaCredentialDisabledEvent(MfaCredentialId CredentialId, UserId UserId, DateTime DisabledAtUtc) : DomainEvent;

public record MfaChallengeCreatedEvent(
    MfaLoginChallengeId ChallengeId,
    UserId UserId,
    MfaChallengePurpose Purpose,
    DateTime ExpiresAtUtc) : DomainEvent;

public record MfaChallengeFailedEvent(MfaLoginChallengeId ChallengeId, UserId UserId, int Attempts) : DomainEvent;

public record MfaChallengeLockedEvent(MfaLoginChallengeId ChallengeId, UserId UserId, int Attempts) : DomainEvent;

public record MfaChallengeCompletedEvent(MfaLoginChallengeId ChallengeId, UserId UserId, DateTime CompletedAtUtc) : DomainEvent;