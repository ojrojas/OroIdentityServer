// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Services.OroIdentityServer.Core.Models;

public class IdentificationType : BaseEntity<Guid>, IAuditableEntity, IAggregateRoot
{
    private IdentificationType()
    {
        IdentificationTypeName = default!;
    }

    public IdentificationType(IdentificationTypeName identificationName)
    {
        IdentificationTypeName = identificationName;
    }
    
    public IdentificationTypeName IdentificationTypeName { get; private set; }
}