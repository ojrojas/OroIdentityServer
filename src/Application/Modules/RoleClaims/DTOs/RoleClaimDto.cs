namespace OroIdentityServer.Application.Modules.RoleClaims.DTOs;

public sealed record RoleClaimDto(Guid Id, string ClaimType, string ClaimValue, bool IsActive);