// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Application.Modules.Tenants.Interfaces;

/// <summary>
/// Provisions the PostgreSQL schema for a new tenant.
/// Implemented in the Infrastructure layer.
/// </summary>
public interface ITenantSchemaProvisioner
{
    Task ProvisionSchemaAsync(string slug, CancellationToken cancellationToken = default);
}
