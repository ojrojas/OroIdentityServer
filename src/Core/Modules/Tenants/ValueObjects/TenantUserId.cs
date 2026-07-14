// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Core.Modules.Tenants.ValueObjects;

public sealed record TenantUserId(Guid Value)
{
    public static TenantUserId New() => new(Guid.CreateVersion7());
    public static TenantUserId From(Guid value) => new(value);
}
