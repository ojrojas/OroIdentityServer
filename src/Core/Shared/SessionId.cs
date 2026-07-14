// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Core.Shared;

public sealed record SessionId(Guid Value)
{
    public static SessionId New(Guid? value) => new(value ?? Guid.CreateVersion7());
    public static SessionId From(Guid value) => new(value);
}
