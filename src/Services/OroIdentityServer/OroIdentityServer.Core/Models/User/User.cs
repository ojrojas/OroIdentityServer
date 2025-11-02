// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Services.OroIdentityServer.Core.Models;

[Index(nameof(Email), IsUnique = true)]
[Index(nameof(UserName), IsUnique = true)]
public class User : BaseEntity<Guid>, IAuditableEntity, IAggregateRoot
{
    public string Name { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string MiddleName { get; set; } = string.Empty;
    public required string UserName { get; set; }
    public required string Email { get; set; }
    public required string Identification {get;set;} = string.Empty;
    public Guid IdentificationTypeId { get; set; }
    public string NormalizedEmail { get; set; } = string.Empty;
    public string NormalizedUserName { get; set; } = string.Empty;
    public IdentificationType? IdentificationType { get; set; }

    public SecurityUser SecurityUserId {get;set;}
    public SecurityUser SecurityUser {get;set;}
}


public class SecurityUser : BaseEntity<Guid>
{
    public required string PasswordHash { get; set; }
    public string SecurityStamp { get; set; } = string.Empty;
    public Guid ConcurrencyStamp { get; set; }
    public DateTime? LockoutEnd { get; set; }
    public bool LockoutEnabled { get; set; } = true;
    public int AccessFailedCount { get; set; } = 0;
}
