namespace OroIdentityServer.Application.Modules.Users.DTOs;

public sealed record UserRoleDto
{
    public Guid? UserId { get; private set; }
    public Guid? RoleId { get; private set; }
}