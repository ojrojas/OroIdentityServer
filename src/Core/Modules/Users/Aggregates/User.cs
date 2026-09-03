// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Core.Modules.Users.Aggregates;

public class User : AggregateRoot<UserId>, IAuditableEntity
{
    public bool IsActive { get; private set; } = true;
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;
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

    public SecurityUserId? SecurityUserId { get; set; }
    public SecurityUser? SecurityUser { get; set; }
  
    private readonly IList<UserRole> _roles = [];
    public IReadOnlyCollection<UserRole> Roles => _roles.AsReadOnly();

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

    public void AddRole(UserRole role)
    {
        if (_roles.Any(r => r.RoleId == role.RoleId))
            throw new InvalidOperationException("Role already assigned to user.");

        _roles.Add(role);
    }

    public void RemoveRole(UserRole role)
    {
        var existing = _roles.FirstOrDefault(r => r.RoleId == role.RoleId);
        if (existing is null)
            throw new InvalidOperationException("Role not found.");

        _roles.Remove(existing);
    }

    public void Deactivate()
    {
        if (!IsActive)
            throw new InvalidOperationException("User is already inactive.");
        IsActive = false;
        RaiseDomainEvent(new UserDeactivatedEvent(Id));
    }

    public void Activate()
    {
        if (IsActive)
            throw new InvalidOperationException("User is already active.");
        IsActive = true;
        RaiseDomainEvent(new UserActivatedEvent(Id));
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
