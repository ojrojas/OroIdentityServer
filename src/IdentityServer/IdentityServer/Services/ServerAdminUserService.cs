using System.Net;
using System.Security.Claims;
using BuildingBlocks.CQRS.Abstractions;
using IdentityServer.Client.Interfaces;
using IdentityServer.Client.Models;
using IdentityServer.Client.Models.Users;
using IdentityServer.Client.Services;
using OroIdentityServer.Application.Modules.Roles.Queries;
using OroIdentityServer.Application.Modules.Users.Commands;
using OroIdentityServer.Application.Modules.Users.Queries;
using OroIdentityServer.Core.Modules.Tenants.Repositories;
using OroIdentityServer.Core.Modules.Tenants.ValueObjects;
using OroIdentityServer.Core.Modules.Users.Aggregates;
using OroIdentityServer.Core.Modules.Users.Entities;
using OroIdentityServer.Core.Shared;
using OroIdentityServer.Server.Authentication;

namespace IdentityServer.Services;

/// <summary>
/// Server-side implementation of IAdminUserService: called by the /api/users minimal API endpoint
/// (not injected into Razor components), talks to the rest of the application via CQRS dispatchers
/// instead of over HTTP, and maps the result into the same client-facing models the HTTP-based
/// AdminUserService would deserialize from the wire.
/// </summary>
public class ServerAdminUserService(
    IQueryDispatcher queryDispatcher,
    ICommandDispatcher commandDispatcher,
    IHttpContextAccessor httpContextAccessor,
    ICurrentTenantContext tenantContext,
    ITenantRepository tenantRepository) : IAdminUserService
{
    private const string AdministratorRoleName = "Administrator";
    public async Task<ApiResponse<IEnumerable<UserModel>>?> GetUsersAsync(CancellationToken ct = default)
    {
        var result = await queryDispatcher.SendAsync(new GetUsersQuery(tenantContext.CurrentTenantId), ct);
        return new ApiResponse<IEnumerable<UserModel>>
        {
            Data = result.Data?.Select(MapUser).ToList() ?? [],
            StatusCode = result.StatusCode,
            Message = result.Message,
            Errors = result.Errors
        };
    }

    public async Task<HttpResponseMessage> CreateUserAsync(CreateUserRequest request, CancellationToken ct = default)
    {
        var caller = httpContextAccessor.HttpContext?.User;
        var callerIsMasterAdmin = caller?.HasClaim(AdminPasswordSignInService.IsMasterAdminClaimType, "true") == true;
        if (!callerIsMasterAdmin)
        {
            if (!await IsAccessibleTenantAsync(caller, request.TenantId, ct))
                return new HttpResponseMessage(HttpStatusCode.Forbidden);
        }

        var command = new CreateUserCommand(
            request.Name, request.MiddleName, request.LastName, request.UserName, request.Email,
            request.Password, request.Identification, request.IdentificationTypeId, request.TenantId);

        var result = await commandDispatcher.SendAsync(command, ct);
        return HttpResponseMessageFactory.FromResult(result, HttpStatusCode.Created);
    }

    public async Task<HttpResponseMessage> UpdateUserAsync(Guid id, UpdateUserRequest request, CancellationToken ct = default)
    {
        var caller = httpContextAccessor.HttpContext?.User;
        var callerIsMasterAdmin = caller?.HasClaim(AdminPasswordSignInService.IsMasterAdminClaimType, "true") == true;
        if (!callerIsMasterAdmin)
        {
            if (!await IsAccessibleTenantAsync(caller, request.TenantId, ct))
                return new HttpResponseMessage(HttpStatusCode.Forbidden);
        }

        var command = new UpdateUserCommand(
            id, request.Name, request.MiddleName, request.LastName, request.UserName, request.Email,
            request.Password, request.Identification, request.IdentificationTypeId, request.TenantId);

        var response = await commandDispatcher.SendAsync(command, ct);
        return new HttpResponseMessage((HttpStatusCode)response.StatusCode);
    }

    public async Task<HttpResponseMessage> DeleteUserAsync(Guid id, CancellationToken ct = default)
    {
        var result = await commandDispatcher.SendAsync(new DeleteUserCommand(id), ct);
        return HttpResponseMessageFactory.FromResult(result, HttpStatusCode.NoContent);
    }

    public async Task<HttpResponseMessage> AssignRolesToUserAsync(Guid userId, AssignRolesRequest request, CancellationToken ct = default)
    {
        var caller = httpContextAccessor.HttpContext?.User;
        var callerIsMasterAdmin = caller?.HasClaim(AdminPasswordSignInService.IsMasterAdminClaimType, "true") == true;
        var callerIsAdmin = caller?.IsInRole(TenantRole.Admin) == true || caller?.IsInRole(TenantRole.Administrator) == true;

        if (!callerIsAdmin)
        {
            return new HttpResponseMessage(HttpStatusCode.Forbidden);
        }

        if (!callerIsMasterAdmin)
        {
            // Tenant admin: can only act on users in the caller's home tenant. TenantUser
            // membership no longer carries a per-tenant role, so authorisation is just
            // "target tenant == caller's home tenant".
            var target = await queryDispatcher.SendAsync(new GetUserByIdQuery(userId), ct);
            if (target.Data is null)
            {
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }

            var callerId = new UserId(Guid.Parse(caller!.FindFirstValue(ClaimTypes.NameIdentifier)!));
            var callerUser = await queryDispatcher.SendAsync(new GetUserByIdQuery(callerId.Value), ct);
            if (callerUser.Data?.TenantId is null ||
                target.Data.TenantId is null ||
                callerUser.Data.TenantId.Value != target.Data.TenantId.Value)
            {
                return new HttpResponseMessage(HttpStatusCode.Forbidden);
            }
        }

        var result = await commandDispatcher.SendAsync(new AssignRolesToUserCommand(userId, request.RoleIds), ct);
        return HttpResponseMessageFactory.FromResult(result, HttpStatusCode.OK);
    }

    public async Task<HttpResponseMessage> LockUserAsync(Guid userId, CancellationToken ct = default)
    {
        var result = await commandDispatcher.SendAsync(new LockUserCommand(userId), ct);
        return HttpResponseMessageFactory.FromResult(result, HttpStatusCode.OK);
    }

    public async Task<HttpResponseMessage> UnlockUserAsync(Guid userId, CancellationToken ct = default)
    {
        var result = await commandDispatcher.SendAsync(new UnlockUserCommand(userId), ct);
        return HttpResponseMessageFactory.FromResult(result, HttpStatusCode.OK);
    }

    private async Task<bool> IsAccessibleTenantAsync(ClaimsPrincipal? caller, Guid targetTenantId, CancellationToken ct)
    {
        if (caller?.Identity?.IsAuthenticated != true) return false;

        // Master admin can touch any tenant; everyone else can only touch their own home tenant.
        if (caller.HasClaim(AdminPasswordSignInService.IsMasterAdminClaimType, "true")) return true;

        var callerIdClaim = caller.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(callerIdClaim, out var callerGuid)) return false;

        var callerUser = await queryDispatcher.SendAsync(new GetUserByIdQuery(callerGuid), ct);
        return callerUser.Data?.TenantId?.Value == targetTenantId;
    }

    private static UserModel MapUser(User user) => new(
        user.Id!.Value,
        user.Name,
        user.MiddleName,
        user.LastName,
        user.UserName,
        user.Email,
        user.Identification,
        user.IdentificationTypeId?.Value,
        user.NormalizedEmail,
        user.NormalizedUserName,
        user.TenantId?.Value,
        user.SecurityUserId?.Value,
        user.SecurityUser?.IsLockedOut() ?? false,
        user.SecurityUser?.LockoutEnd,
        user.Roles.Select(MapUserRole).ToList(),
        user.CreatedAtUtc);

    private static UserRoleModel MapUserRole(UserRole role) => new(role.UserId?.Value, role.RoleId?.Value);

    public async Task<ApiResponse<UserModel>?> GetUserByIdAsync(Guid Id, CancellationToken ct = default)
    {
        var result = await queryDispatcher.SendAsync(new GetUserByIdQuery(Id), ct);
         return new ApiResponse<UserModel>
         {
            Data  = result.Data is null ? null : MapUser(result.Data),
            StatusCode = result.StatusCode,
            Message = result.Message,
            Errors = result.Errors  
         };
    }
}
