// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Services.OroIdentityServer.Core.Models;

public class User : BaseEntity<User, UserId>, IAuditableEntity, IAggregateRoot
{
    public User(
        UserId? id,
        string name,
        string middleName,
        string lastName,
        string userName,
        string email,
        string identification,
        IdentificationTypeId identificationTypeId, 
        TenantId tenantId)
    {
        Id = id ?? UserId.New();
        Name = name;
        MiddleName = middleName;
        LastName = lastName;
        UserName = userName;
        Email = email;
        Identification = identification;
        IdentificationTypeId = identificationTypeId;
        TenantId = tenantId;
        NormalizedEmail = NormalizedEmailFrom(email);
        NormalizedUserName = NormalizedUserNameFrom(userName);
        RaiseDomainEvent(new UserCreateEvent(
            Id,
            name,
            middleName,
            lastName,
            userName,
            email,
            identification,
            identificationTypeId, 
            tenantId));
    }

    public static string NormalizedEmailFrom(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be null or empty.");
        return email.ToUpperInvariant();
    }
    public static string NormalizedUserNameFrom(string userName)
    {
        if (string.IsNullOrWhiteSpace(userName))
            throw new ArgumentException("UserName cannot be null or empty.");
        return userName.ToUpperInvariant();
    }

    private readonly IList<UserRole> _roles = [];

    public string? Name { get; private set; } = string.Empty;
    public string? LastName { get; private set; } = string.Empty;
    public string? MiddleName { get; set; } = string.Empty;
    public string? UserName { get; private set; }
    public string? Email { get; private set; }
    public string? Identification { get; private set; } = string.Empty;
    public IdentificationTypeId? IdentificationTypeId { get; private set; }
    public string? NormalizedEmail { get; set; } = string.Empty;
    public string? NormalizedUserName { get; set; } = string.Empty;
    public IdentificationType? IdentificationType { get; set; }

    public TenantId? TenantId { get; private set; }
    public Tenant? Tenant { get; set; }

    public IReadOnlyCollection<UserRole> Roles => _roles.AsReadOnly();

    public SecurityUserId? SecurityUserId { get; set; }
    public SecurityUser? SecurityUser { get; set; }

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
        if (!Email.Contains('@'))
            throw new ArgumentException("Email must be valid.");
    }

    public void AssignSecurityUser(SecurityUser securityUser)
    {
        if (SecurityUser != null)
            throw new InvalidOperationException("SecurityUser is already assigned.");

        SecurityUser = securityUser ?? throw new ArgumentNullException(nameof(securityUser), "SecurityUser cannot be null.");
        SecurityUserId = securityUser.Id;

        RaiseDomainEvent(new SecurityUserAssignedEvent(Id, securityUser.Id));
    }

    // Add Create method
    public static User Create(
        string userName, 
        string email, 
        string name, 
        string middleName, 
        string lastName, 
        string identification,
        IdentificationTypeId identificationTypeId,
        TenantId tenantId)
    {
        if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("UserName and Email cannot be null or empty.");

        return new User(
            UserId.New(),
            name,
            middleName,
            lastName,
            userName,
            email,
            identification,
            identificationTypeId, 
            tenantId
        );
    }

    // Add UpdateDetails method
    public void UpdateDetails(
        string name, 
        string middleName, 
        string lastName, 
        string userName, 
        string email, 
        string identification, 
        IdentificationTypeId identificationTypeId,
        TenantId tenantId)
    {
        Name = name;
        MiddleName = middleName;
        LastName = lastName;
        UserName = userName;
        Email = email;
        Identification = identification;
        IdentificationTypeId = identificationTypeId;
        TenantId = tenantId;
    }
}
