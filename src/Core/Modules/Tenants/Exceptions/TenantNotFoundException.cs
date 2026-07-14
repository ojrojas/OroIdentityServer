// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroAccoOroIdentityServeruntants.Core.Modules.Tenants.Exceptions;

public sealed class TenantNotFoundException : Exception
{
    public TenantNotFoundException(Guid tenantId)
        : base($"Tenant with id '{tenantId}' was not found.") { }

    public TenantNotFoundException(string slug)
        : base($"Tenant with slug '{slug}' was not found.") { }
}
