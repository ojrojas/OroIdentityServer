using Microsoft.AspNetCore.Mvc;
using IdentityServer.Client.Models.Users;
using IdentityServer.Client.Interfaces;

namespace OroIdentityServer.Server.Endpoints;

public static partial class AdminApiEndpoints
{
    private static void MapUsers(this RouteGroupBuilder api)
    {
        var g = api.MapGroup("/users");

        g.MapGet("/", async ([FromServices] IAdminUserService service, CancellationToken ct)
            => Results.Ok(await service.GetUsersAsync(ct)));

        g.MapPost("/", async (
            [FromBody] CreateUserRequest request,
            [FromServices] IAdminUserService service,
            CancellationToken ct) => await ToResultAsync(await service.CreateUserAsync(request, ct), ct));

        g.MapPut("/{id:guid}", async (
            Guid id,
            [FromBody] UpdateUserRequest request,
            [FromServices] IAdminUserService service,
            CancellationToken ct) => await ToResultAsync(await service.UpdateUserAsync(id, request, ct), ct));

        g.MapDelete("/{id:guid}", async (
            Guid id,
            [FromServices] IAdminUserService service,
            CancellationToken ct) => await ToResultAsync(await service.DeleteUserAsync(id, ct), ct));

        g.MapPut("/{id:guid}/roles", async (
            Guid id,
            [FromBody] AssignRolesRequest request,
            [FromServices] IAdminUserService service,
            CancellationToken ct) => await ToResultAsync(await service.AssignRolesToUserAsync(id, request, ct), ct));

        g.MapPost("/{id:guid}/lock", async (
            Guid id,
            [FromServices] IAdminUserService service,
            CancellationToken ct) => await ToResultAsync(await service.LockUserAsync(id, ct), ct));

        g.MapPost("/{id:guid}/unlock", async (
            Guid id,
            [FromServices] IAdminUserService service,
            CancellationToken ct) => await ToResultAsync(await service.UnlockUserAsync(id, ct), ct));
    }
}
