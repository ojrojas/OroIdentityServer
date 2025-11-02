// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Services.OroIdentityServer.Api.Endpoints;

public static class UsersCommandsEndpoints
{
    public static RouteGroupBuilder MapUsersCommandsEndpointsV1(this IEndpointRouteBuilder routeBuilder)
    {
        var api = routeBuilder.MapGroup(string.Empty);

        api.MapPost("/create", CreateUser);
        api.MapPatch("/update", UpdateUser);
        api.MapDelete("/delete/{id}", DeleteUser);

        return api;
    }

     private static async Task<Results<Ok, BadRequest<string>, ProblemHttpResult>> DeleteUser(
        HttpContext context,
        [FromRoute] Guid id, 
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteUserCommand(id), cancellationToken);
        return TypedResults.Ok();
    }

    private static async Task<Results<Ok<UpdateUserResponse>, BadRequest<string>, ProblemHttpResult>> UpdateUser(
        HttpContext context,
        [FromServices] ISender sender,
        [FromBody] UpdateUserCommand request,
        CancellationToken cancellationToken)
    {
        return TypedResults.Ok(await sender.Send(request, cancellationToken));
    }

    private static async Task<Results<Ok, BadRequest<string>, ProblemHttpResult>> CreateUser(
        HttpContext context,
        [FromServices] ISender sender,
        [FromBody] CreateUserCommand request,
        CancellationToken cancellationToken)
    {
        await sender.Send(request, cancellationToken);
        return TypedResults.Ok();
    }
}