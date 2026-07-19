using System.Net;
using BuildingBlocks.CQRS.Abstractions;
using IdentityServer.Client.Interfaces;
using IdentityServer.Client.Models;
using IdentityServer.Client.Models.Users;
using OroIdentityServer.Application.Modules.Users.Commands;
using OroIdentityServer.Application.Modules.Users.Queries;
using OroIdentityServer.Core.Modules.Users.Aggregates;
using OroIdentityServer.Core.Modules.Users.Entities;

namespace IdentityServer.Services;

/// <summary>
/// Server-side implementation of IAdminUserService: called by the /api/users minimal API endpoint
/// (not injected into Razor components), talks to the rest of the application via CQRS dispatchers
/// instead of over HTTP, and maps the result into the same client-facing models the HTTP-based
/// AdminUserService would deserialize from the wire.
/// </summary>
public class ServerAdminUserService(IQueryDispatcher queryDispatcher, ICommandDispatcher commandDispatcher) : IAdminUserService
{
    public async Task<ApiResponse<IEnumerable<UserModel>>?> GetUsersAsync(CancellationToken ct = default)
    {
        var result = await queryDispatcher.SendAsync(new GetUsersQuery(), ct);
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
        var command = new CreateUserCommand(
            request.Name, request.MiddleName, request.LastName, request.UserName, request.Email,
            request.Password, request.Identification, request.IdentificationTypeId, request.TenantId);

        var result = await commandDispatcher.SendAsync(command, ct);
        return HttpResponseMessageFactory.FromResult(result, HttpStatusCode.Created);
    }

    public async Task<HttpResponseMessage> UpdateUserAsync(Guid id, UpdateUserRequest request, CancellationToken ct = default)
    {
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
        user.Roles.Select(MapUserRole).ToList());

    private static UserRoleModel MapUserRole(UserRole role) => new(role.UserId?.Value, role.RoleId?.Value);
}
