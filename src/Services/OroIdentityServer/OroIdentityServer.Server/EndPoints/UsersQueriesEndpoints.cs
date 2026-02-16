// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Services.OroIdentityServer.Server.Endpoints;

public static class UsersQueriesEndpoints
{
    extension(IEndpointRouteBuilder routeBuilder)
    {
        public RouteGroupBuilder MapUsersQueriesEndpointsV1()
        {
            var api = routeBuilder.MapGroup("users");

            api.MapGet("/getuserinfo", GetUserInfo)
                .WithName("GetUserInfo");

            api.MapGet(string.Empty, GetUsers)
                .WithName("GetUsers");

            api.MapGet("/getuserbyemail/{email}", GetUserByEmail)
                .WithName("GetUserByEmail");

            api.MapGet("/{id:guid}", GetUserById)
                .WithName("GetUserById");

            api.RequireAuthorization([new AuthorizeAttribute
            {
                AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme
            }]);
            return api;
        }
    }

    private static async Task<Results<Ok<GetUserByIdQueryResponse>, BadRequest<string>, ProblemHttpResult>> 
    GetUserById(
           HttpContext context,
           [FromRoute] Guid id,
           [FromServices] ISender sender,
           CancellationToken cancellationToken
           )
    {
        return TypedResults.Ok(await sender.Send(new GetUserByIdQuery(new(id)), cancellationToken));
    }

    private static async Task<Results<Ok<GetUserByEmailResponse>, BadRequest<string>, ProblemHttpResult>> 
    GetUserByEmail(
           HttpContext context,
           [FromRoute] string email,
           [FromServices] ISender sender,
           CancellationToken cancellationToken
           )
    {
        return TypedResults.Ok(await sender.Send(new GetUserByEmailQuery(email), cancellationToken));
    }

    private static async Task<Results<Ok<GetUsersQueryResponse>, BadRequest<string>, ProblemHttpResult>> 
    GetUsers(
           HttpContext context,
           [FromServices] ISender sender,
           CancellationToken cancellationToken)
    {
        return TypedResults.Ok(await sender.Send(new GetUsersQuery(), cancellationToken));
    }

    private static async Task<Results<Ok<GetUserByIdQueryResponse>, BadRequest<string>, ProblemHttpResult>> 
    GetUserInfo(
           HttpContext context,
           [FromServices] ISender sender,
           CancellationToken cancellationToken
       )
    {
        // Try to use the authenticated user available on the HttpContext.
        // Prefer `NameIdentifier` claim, then `sub`, then `user_id`.
        var principal = context.User;
        if (principal?.Identity?.IsAuthenticated != true)
        {
            return TypedResults.Problem("User is not authenticated", statusCode: StatusCodes.Status401Unauthorized);
        }

        var idClaim = principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                      ?? principal.FindFirst("sub")?.Value
                      ?? principal.FindFirst("user_id")?.Value;

        if (string.IsNullOrWhiteSpace(idClaim) || !Guid.TryParse(idClaim, out var userGuid))
        {
            return TypedResults.BadRequest("Invalid or missing user identifier in claims.");
        }

        return TypedResults.Ok(await sender.Send(new GetUserByIdQuery(new(userGuid)), cancellationToken));

    }
}