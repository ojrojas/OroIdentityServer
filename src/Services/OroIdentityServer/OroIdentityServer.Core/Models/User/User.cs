// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Services.OroIdentityServer.Core.Models;

public class User : AggregateRoot<Guid>, IAuditableEntity
{
    public User(Guid Id) : base(Id)
    {
    }

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

    public readonly ICollection<UserRole

    public Guid SecurityUserId {get;set;}
    public SecurityUser? SecurityUser {get;set;}
}
