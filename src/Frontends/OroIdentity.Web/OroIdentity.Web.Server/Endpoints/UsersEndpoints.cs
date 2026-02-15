using Microsoft.AspNetCore.Mvc;
using OroIdentity.Web.Client.Interfaces;
using OroIdentity.Web.Client.Models;

namespace OroIdentity.Web.Server.Endpoints;

public static class UsersEndpoints
{
    public static RouteGroupBuilder MapUsersEndpointsV1(this IEndpointRouteBuilder routeBuilder)
    {
        var group = routeBuilder.MapGroup("api/v1/users")
            .WithTags("Users");

        group.MapGet(string.Empty, GetAllUsers);
        group.MapGet("{userId:guid}", GetUserById);

        group.MapDelete("{userId:guid}", DeleteUserById);
        group.MapPost(string.Empty, CreateUser);
        group.MapPut(string.Empty, UpdateUser);

        return group;
    }

    private static async Task DeleteUserById(
        HttpContext context,
        Guid userId,
        [FromServices] IUsersService service,
        CancellationToken cancellationToken
        )
    {
        await service.DeleteUserAsync(userId.ToString(), cancellationToken);
    }

    private static async Task<IResult> UpdateUser(
        HttpContext context,
        [FromServices] IUsersService service,
        [FromBody] UserViewModel user,
        CancellationToken cancellationToken)
    {
        await service.UpdateUserAsync(user, cancellationToken);
        return TypedResults.Ok();
    }

    private static async Task<IResult> CreateUser(
        HttpContext context,
        [FromServices] IUsersService service,
        [FromBody] UserViewModel user,
        CancellationToken cancellationToken)
    {
        await service.CreateUserAsync(user, cancellationToken);
        return TypedResults.Ok();
    }

    private static async Task<UserViewModel?> GetUserById(
        HttpContext context,
        Guid userId,
        [FromServices] IUsersService service,
        CancellationToken cancellationToken
        )
    {
        return await service.GetUserByIdAsync(userId.ToString(), cancellationToken);
    }

    private static async Task<BaseResponseViewModel<IEnumerable<UserViewModel>>?> GetAllUsers(
        HttpContext context,
        [FromServices] IUsersService service,
        CancellationToken cancellationToken
        )
    {
        return await service.GetAllUsersAsync(cancellationToken);
    }
}