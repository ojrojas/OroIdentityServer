// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Services.OroIdentityServer.Core.Models;

public class User : AggregateRoot<UserId>, IAuditableEntity
{
    public User(
        UserId id,
        string name,
        string middleName,
        string lastName,
        string userName,
        string email,
        string identification,
        IdentificationTypeId identificationTypeId) : base(id)
    {
        Id = id;
        Name = name;
        MiddleName = middleName;
        LastName = lastName;
        UserName = userName;
        Email = email;
        Identification = identification;
        IdentificationTypeId = identificationTypeId;
        NormalizedEmail = email.ToUpperInvariant();
        NormalizedUserName = userName.ToUpperInvariant();
    }

    private readonly IList<UserRole> _roles = [];

    public string Name { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string MiddleName { get; set; } = string.Empty;
    public string UserName { get; private set; }
    public string Email { get; private set; }
    public string Identification { get; private set; } = string.Empty;
    public IdentificationTypeId IdentificationTypeId { get; private set; }
    public string NormalizedEmail { get; set; } = string.Empty;
    public string NormalizedUserName { get; set; } = string.Empty;
    public IdentificationType? IdentificationType { get; set; }

    public IReadOnlyCollection<UserRole> Roles => _roles.AsReadOnly();

    public Guid SecurityUserId { get; set; }
    public SecurityUser? SecurityUser { get; set; }

    // Add repository interface for User
    public interface IUserRepository
    {
        User? GetById(UserId id);
        void Add(User user);
        void Update(User user);
        void Remove(User user);
    }

    // Add domain logic to User
    public void AddRole(UserRole role)
    {
        if (_roles.Any(r => r.RoleId == role.RoleId))
            throw new InvalidOperationException("Role already assigned to user.");

        _roles.Add(role);
    }

    public void RemoveRole(UserRole role)
    {
        if (!_roles.Remove(role))
            throw new InvalidOperationException("Role not found.");
    }

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Name))
            throw new ArgumentException("Name cannot be empty.");
        if (string.IsNullOrWhiteSpace(Email))
            throw new ArgumentException("Email cannot be empty.");
        if (!Email.Contains("@"))
            throw new ArgumentException("Email must be valid.");
    }
}
