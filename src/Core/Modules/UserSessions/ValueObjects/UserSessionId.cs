// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Core.Modules.UserSessions.ValueObjects;

public sealed record UserSessionId(Guid Value)
{
    public static UserSessionId New() => new(Guid.CreateVersion7());
    public static UserSessionId From(Guid value) => new(value);
}
