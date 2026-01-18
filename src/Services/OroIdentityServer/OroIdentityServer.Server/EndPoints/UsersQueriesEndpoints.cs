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
            var api = routeBuilder.MapGroup(string.Empty);

            api.MapGet("/getuserinfo", GetUserInfo)
                .WithName("GetUserInfo");

            api.MapGet("/users", GetUsers)
                .WithName("GetUsers");

            api.MapGet("/getuserbyemail/{email}", GetUserByEmail)
                .WithName("GetUserByEmail");

            api.MapGet("/getuserbyid/{id}", GetUserById)
                .WithName("GetUserById");

            api.RequireAuthorization([new AuthorizeAttribute
            {
                AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme
            }]);
            return api;
        }
    }

    private static async Task<Results<Ok<GetUserByIdQueryResponse>, BadRequest<string>, ProblemHttpResult>> GetUserById(
           HttpContext context,
           [FromRoute] Guid id,
           [FromServices] ISender sender,
           CancellationToken cancellationToken
           )
    {
        return TypedResults.Ok(await sender.Send(new GetUserByIdQuery(new(id)), cancellationToken));
    }

    private static async Task<Results<Ok<GetUserByEmailResponse>, BadRequest<string>, ProblemHttpResult>> GetUserByEmail(
           HttpContext context,
           [FromRoute] string email,
           [FromServices] ISender sender,
           CancellationToken cancellationToken
           )
    {
        return TypedResults.Ok(await sender.Send(new GetUserByEmailQuery(email), cancellationToken));
    }

    private static async Task<Results<Ok<GetUsersQueryResponse>, BadRequest<string>, ProblemHttpResult>> GetUsers(
           HttpContext context,
           [FromServices] ISender sender,
           CancellationToken cancellationToken)
    {
        return TypedResults.Ok(await sender.Send(new GetUsersQuery(), cancellationToken));
    }

    private static async Task<Results<Ok<GetUserByIdQueryResponse>, BadRequest<string>, ProblemHttpResult>> GetUserInfo(
           HttpContext context,
           [FromServices] ISender sender,
           CancellationToken cancellationToken
       )
    {
        //    var result = await context.AuthenticateAsync(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
        // var response = await services.GetUserByIdAsync(new(Guid.Parse(result.Principal.GetUserId())));

        // return TypedResults.Ok(await sender.Send(new GetUserByIdQuery(id), cancellationToken));
        throw new NotImplementedException();

    }
}