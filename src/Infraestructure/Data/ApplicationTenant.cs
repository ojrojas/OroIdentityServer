// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Infraestructure.Data;

public class ApplicationTenant : IAggregateRoot
{
    // Parameterless constructor for EF Core
    private ApplicationTenant() { }

    public ApplicationTenant(string clientId, TenantId tenantId)
    {
        ClientId = clientId;
        TenantId = tenantId;
    }

    public string ClientId { get; set; } = string.Empty;
    public TenantId TenantId { get; set; } = default!;
}
