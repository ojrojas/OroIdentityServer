// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Application.Queries;

public record GetTenantQuery(TenantId Id) : IQuery<TenantDto>
{
    public Guid CorrelationId() => Guid.NewGuid();
}

public record TenantDto(TenantId Id, string Name, bool IsActive, DateTime CreatedAtUtc);
