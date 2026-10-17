// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Core.Modules.Mfa.ValueObjects;

public sealed record MfaLoginChallengeId(Guid Value)
{
    public static MfaLoginChallengeId New() => new(Guid.CreateVersion7());
    public static MfaLoginChallengeId From(Guid value) => new(value);
}