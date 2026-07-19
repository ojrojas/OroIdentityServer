using System.Net;
using BuildingBlocks.CQRS.Abstractions;
using IdentityServer.Client.Interfaces;
using IdentityServer.Client.Models;
using IdentityServer.Client.Models.Permissions;
using OroIdentityServer.Application.Modules.Permissions.Commands;
using OroIdentityServer.Application.Modules.Permissions.DTOs;
using OroIdentityServer.Application.Modules.Permissions.Queries;

namespace IdentityServer.Services;

public class ServerAdminPermissionService(IQueryDispatcher queryDispatcher, ICommandDispatcher commandDispatcher) : IAdminPermissionService
{
    public async Task<ApiResponse<IEnumerable<PermissionModel>>?> GetPermissionsAsync(CancellationToken ct = default)
    {
        var result = await queryDispatcher.SendAsync(new GetPermissionsQuery(), ct);
        return new ApiResponse<IEnumerable<PermissionModel>>
        {
            Data = result.Data?.Select(MapPermission).ToList() ?? [],
            StatusCode = result.StatusCode,
            Message = result.Message,
            Errors = result.Errors
        };
    }

    public async Task<ApiResponse<PermissionModel>?> GetPermissionByIdAsync(Guid id, CancellationToken ct = default)
    {
        var result = await queryDispatcher.SendAsync(new GetPermissionByIdQuery(id), ct);
        return new ApiResponse<PermissionModel>
        {
            Data = result.Data is null ? null : MapPermission(result.Data),
            StatusCode = result.StatusCode,
            Message = result.Message,
            Errors = result.Errors
        };
    }

    public async Task<HttpResponseMessage> CreatePermissionAsync(CreatePermissionRequest request, CancellationToken ct = default)
    {
        var command = new CreatePermissionCommand(
            request.PermissionId, request.Provider, request.Description, request.Action, request.Resource, request.IsSystem);
        var result = await commandDispatcher.SendAsync(command, ct);
        return HttpResponseMessageFactory.FromResult(result, HttpStatusCode.Created);
    }

    public async Task<HttpResponseMessage> UpdatePermissionAsync(Guid id, UpdatePermissionRequest request, CancellationToken ct = default)
    {
        var command = new UpdatePermissionCommand(
            id, request.Provider, request.Description, request.Action, request.Resource, request.IsSystem);
        var result = await commandDispatcher.SendAsync(command, ct);
        return HttpResponseMessageFactory.FromResult(result, HttpStatusCode.NoContent);
    }

    public async Task<HttpResponseMessage> DeletePermissionAsync(Guid id, CancellationToken ct = default)
    {
        var result = await commandDispatcher.SendAsync(new DeletePermissionCommand(id), ct);
        return HttpResponseMessageFactory.FromResult(result, HttpStatusCode.NoContent);
    }

    private static PermissionModel MapPermission(PermissionDto permission) => new(
        permission.PermissionId, permission.Provider, permission.Description, permission.Action, permission.Resource, permission.IsSystem);
}
