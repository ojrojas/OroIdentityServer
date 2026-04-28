namespace OroIdentityServer.Application.Modules.Users.DTOs;

public sealed record UserDto
{
    public Guid Id { get; set; }
    public string? Name { get; set; } = string.Empty;
    public string? LastName { get; set; } = string.Empty;
    public string? MiddleName { get; set; } = string.Empty;
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public string? Identification { get; set; } = string.Empty;
    public Guid? IdentificationTypeId { get; set; }
    public string? NormalizedEmail { get; set; } = string.Empty;
    public string? NormalizedUserName { get; set; } = string.Empty;

    public Guid TenantId { get; set; }
    public Guid SecurityUserId { get; set; }
    private readonly IList<UserRoleDto> _roles = [];
    public IReadOnlyCollection<UserRoleDto> Roles => _roles.AsReadOnly();
}
