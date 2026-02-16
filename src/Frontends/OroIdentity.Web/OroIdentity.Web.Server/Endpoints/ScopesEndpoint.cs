using Microsoft.AspNetCore.Mvc;
using OroIdentity.Web.Client.Interfaces;
using OroIdentity.Web.Client.Models;

namespace OroIdentity.Web.Server.Endpoints;

public static class ScopesEndpoints
{
    public static RouteGroupBuilder MapScopesEndpointsV1(this IEndpointRouteBuilder routeBuilder)
    {
        var group = routeBuilder.MapGroup("api/v1/scopes")
            .WithTags("Scopes");

        group.MapGet(string.Empty, GetAllScopes);
        group.MapGet("{name}", GetScopeByName);
        group.MapGet("{scopeId:guid}", GetScopeById);

        group.MapDelete("{scopeId:guid}", DeleteScopeById);
        group.MapPost(string.Empty, CreateScope);
        group.MapPut(string.Empty, UpdateScope);

        return group;
    }

    private static async Task DeleteScopeById(
        HttpContext context,
        Guid scopeId,
        [FromServices] IScopesService service,
        CancellationToken cancellationToken
        )
    {
        await service.DeleteScopeAsync(scopeId.ToString(), cancellationToken);
    }

    private static async Task<IResult> UpdateScope(
        HttpContext context,
        [FromServices] IScopesService service,
        [FromBody] ScopeViewModel scope,
        CancellationToken cancellationToken)
    {
        await service.UpdateScopeAsync(scope, cancellationToken);
        return TypedResults.Ok();
    }

    private static async Task<IResult> CreateScope(
        HttpContext context,
        [FromServices] IScopesService service,
        [FromBody] ScopeViewModel scope,
        CancellationToken cancellationToken)
    {
        await service.CreateScopeAsync(scope, cancellationToken);
        return TypedResults.Ok();
    }

    private static async Task<ScopeViewModel?> GetScopeById(
        HttpContext context,
        Guid scopeId,
        [FromServices] IScopesService service,
        CancellationToken cancellationToken
        )
    {
        return await service.GetScopeByIdAsync(scopeId.ToString(), cancellationToken);
    }

    private static async Task<ScopeViewModel?> GetScopeByName(
        HttpContext context,
        string name,
        [FromServices] IScopesService service,
        CancellationToken cancellationToken
        )
    {
        return await service.GetScopeByNameAsync(name, cancellationToken);
    }

    private static async Task<BaseResponseViewModel<IEnumerable<ScopeViewModel>>?> GetAllScopes(
        HttpContext context,
        [FromServices] IScopesService service,
        CancellationToken cancellationToken
        )
    {
        return await service.GetAllScopesAsync(cancellationToken);
    }
}