// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Application.Modules.Mfa.Queries;

using OroIdentityServer.Core.Modules.Mfa.Enums;

/// <summary>What the pending-MFA pages need to know about the challenge in the cookie.</summary>
public sealed record MfaChallengeLookup(
    bool Found,
    bool IsUsable,
    Guid UserId,
    MfaChallengePurpose Purpose,
    string? ReturnUrl,
    DateTime ExpiresAtUtc)
{
    public static MfaChallengeLookup NotFound { get; } = new(
        Found: false, IsUsable: false, UserId: Guid.Empty,
        Purpose: MfaChallengePurpose.Verify, ReturnUrl: null, ExpiresAtUtc: default);
}

public record GetMfaChallengeQuery(string ChallengeToken) : IQuery<MfaChallengeLookup>
{
    public Guid CorrelationId { get; init; } = Guid.NewGuid();
}

/// <summary>
/// Resolves the pending challenge behind the cookie. The lookup itself never grants anything:
/// it only tells the page which step the user is on (prove it, or enrol first) so it can render
/// the right form.
/// </summary>
public sealed class GetMfaChallengeQueryHandler(
    ILogger<GetMfaChallengeQueryHandler> logger,
    IMfaLoginChallengeRepository challengeRepository) : IQueryHandler<GetMfaChallengeQuery, MfaChallengeLookup>
{
    public async Task<MfaChallengeLookup> HandleAsync(GetMfaChallengeQuery query, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling GetMfaChallengeQuery");

        var challenge = await challengeRepository.GetByTokenAsync(query.ChallengeToken, cancellationToken);
        if (challenge is null)
            return MfaChallengeLookup.NotFound;

        return new MfaChallengeLookup(
            Found: true,
            IsUsable: challenge.CanBeUsed(DateTime.UtcNow),
            UserId: challenge.UserId.Value,
            Purpose: challenge.Purpose,
            ReturnUrl: challenge.ReturnUrl,
            ExpiresAtUtc: challenge.ExpiresAtUtc);
    }
}