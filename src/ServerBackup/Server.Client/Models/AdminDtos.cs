// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Server.Client.Models;

public record ApiEnvelope<T>(T? Data, int StatusCode = 200, string? Message = null, string[]? Errors = null);

public sealed class UserDto
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? LastName { get; set; }
    public string? MiddleName { get; set; }
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public string? Identification { get; set; }
    public Guid? IdentificationTypeId { get; set; }
    public Guid TenantId { get; set; }
}

public class CreateUserModel
{
    public string Name { get; set; } = string.Empty;
    public string MiddleName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Identification { get; set; } = string.Empty;
    public Guid IdentificationTypeId { get; set; }
    public Guid TenantId { get; set; }
}

public sealed class UpdateUserModel : CreateUserModel
{
    public Guid UserId { get; set; }
}

public record RoleDto(Guid Id, bool IsActive, string? Name);
public sealed class CreateRoleModel { public string RoleName { get; set; } = string.Empty; }
public sealed class UpdateRoleModel { public Guid Id { get; set; } public string RoleName { get; set; } = string.Empty; }

public record PermissionDto(Guid PermissionId, string Provider, string? Description, string Action, string Resource, bool IsSystem);
public sealed class PermissionUpsertModel
{
    public Guid PermissionId { get; set; } = Guid.NewGuid();
    public string Provider { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Action { get; set; } = string.Empty;
    public string Resource { get; set; } = string.Empty;
    public bool IsSystem { get; set; }
}

public record TenantDto(Guid Id, string Name, string Slug, bool IsActive, DateTime CreatedAtUtc, int UserCount);
public sealed class CreateTenantModel
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public Guid OwnerId { get; set; }
}
public sealed class UpdateTenantModel { public Guid TenantId { get; set; } public string Name { get; set; } = string.Empty; }

public record IdentificationTypeDto(Guid Id, string Name, bool IsActive, DateTime CreatedAtUtc);
public sealed class IdentificationTypeUpsertModel { public string Name { get; set; } = string.Empty; }

public sealed class UserSessionDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string? Device { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? Location { get; set; }
    public bool IsActive { get; set; }
}

public sealed class ApplicationDto
{
    public string? ClientId { get; set; }
    public string? DisplayName { get; set; }
    public string? ClientType { get; set; }
    public string? ApplicationType { get; set; }
    public string? ClientSecret { get; set; }
    public string? ConsentType { get; set; }
    public List<string> Permissions { get; set; } = new();
    public List<string> Requirements { get; set; } = new();
    public List<string> RedirectUris { get; set; } = new();
    public List<string> PostLogoutRedirectUris { get; set; } = new();
}

public sealed class ScopeDto
{
    public string Name { get; set; } = string.Empty;
    public List<string> Resources { get; set; } = new();
}
