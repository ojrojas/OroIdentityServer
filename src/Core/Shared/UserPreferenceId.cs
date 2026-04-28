// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Core.Shared;

public readonly record struct UserPreferenceId(Guid Value)
{
    public static UserPreferenceId New() => new(Guid.CreateVersion7());
    public static UserPreferenceId From(Guid value) => new(value);
}
