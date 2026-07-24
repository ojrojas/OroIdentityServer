using System.Net;
using BuildingBlocks.CQRS.Abstractions;
using IdentityServer.Client.Interfaces;
using IdentityServer.Client.Models;
using IdentityServer.Client.Models.Roles;
using OroIdentityServer.Application.Modules.Roles.Commands;
using OroIdentityServer.Application.Modules.Roles.DTOs;
using OroIdentityServer.Application.Modules.Roles.Queries;

namespace IdentityServer.Services;

public class ServerAdminRoleService(IQueryDispatcher queryDispatcher, ICommandDispatcher commandDispatcher) : IAdminRoleService
{
    public async Task<ApiResponse<IEnumerable<RoleModel>>?> GetRolesAsync(CancellationToken ct = default)
    {
        var result = await queryDispatcher.SendAsync(new GetRolesQuery(), ct);
        return new ApiResponse<IEnumerable<RoleModel>>
        {
            Data = result.Data?.Select(MapRole).ToList() ?? [],
            StatusCode = result.StatusCode,
            Message = result.Message,
            Errors = result.Errors
        };
    }

    public async Task<ApiResponse<RoleModel>?> GetRoleByIdAsync(Guid id, CancellationToken ct = default)
    {
        var result = await queryDispatcher.SendAsync(new GetRoleByIdQuery(id), ct);
        return new ApiResponse<RoleModel>
        {
            Data = result.Data is null ? null : MapRole(result.Data),
            StatusCode = result.StatusCode,
            Message = result.Message,
            Errors = result.Errors
        };
    }

    public async Task<HttpResponseMessage> CreateRoleAsync(CreateRoleRequest request, CancellationToken ct = default)
    {
        var result = await commandDispatcher.SendAsync(new CreateRoleCommand(request.RoleName), ct);
        return HttpResponseMessageFactory.FromResult(result, HttpStatusCode.Created);
    }

    public async Task<HttpResponseMessage> UpdateRoleAsync(Guid id, UpdateRoleRequest request, CancellationToken ct = default)
    {
        var result = await commandDispatcher.SendAsync(new UpdateRoleCommand(id, request.RoleName), ct);
        return HttpResponseMessageFactory.FromResult(result, HttpStatusCode.NoContent);
    }

    public async Task<HttpResponseMessage> DeleteRoleAsync(Guid id, CancellationToken ct = default)
    {
        var result = await commandDispatcher.SendAsync(new DeleteRoleCommand(id), ct);
        return HttpResponseMessageFactory.FromResult(result, HttpStatusCode.NoContent);
    }

    private static RoleModel MapRole(RoleDto role) => new(
        role.Id,
        role.IsActive,
        role.Name?.Value,
        role.Claims.Select(c => new RolePermissionModel(c.RoleId, c.PermissionId)),
        role.CreatedAtUtc);
}
