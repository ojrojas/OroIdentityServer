namespace OroIdentityServer.Application.Modules.Users.DTOs;

public sealed record UserDto
{
    public string? Name { get; private set; } = string.Empty;
    public string? LastName { get; private set; } = string.Empty;
    public string? MiddleName { get; set; } = string.Empty;
    public string? UserName { get; private set; }
    public string? Email { get; private set; }
    public string? Identification { get; private set; } = string.Empty;
    public Guid? IdentificationTypeId { get; private set; }
    public string? NormalizedEmail { get; set; } = string.Empty;
    public string? NormalizedUserName { get; set; } = string.Empty;

    public Guid? TenantId { get; private set; }
    public Guid? SecurityUserId { get; set; }
    private readonly IList<UserRoleDto> _roles = [];
    public IReadOnlyCollection<UserRoleDto> Roles => _roles.AsReadOnly();
}
