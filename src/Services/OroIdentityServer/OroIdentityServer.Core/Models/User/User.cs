// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using OpenIddict.EntityFrameworkCore.Models;

namespace OroIdentityServer.Services.OroIdentityServer.Core.Models;

public class User : BaseEntity<Guid>, IAuditableEntity, IAggregateRoot
{
    public string Name { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string MiddleName { get; set; } = string.Empty;
    public Guid IdentificationTypeId { get; set; }
    public IdentificationType? IdentificationType { get; set; }
}

