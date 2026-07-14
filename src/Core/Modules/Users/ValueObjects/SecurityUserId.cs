// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Core.Modules.Users.ValueObjects;

public sealed record SecurityUserId(Guid Value)
{
    public static SecurityUserId New() => new(Guid.CreateVersion7());
    public static SecurityUserId From(Guid value) => new(value);
}
