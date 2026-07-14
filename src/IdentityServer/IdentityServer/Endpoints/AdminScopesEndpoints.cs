using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OroCQRS.Core.Interfaces;
using OroIdentityServer.Application.Modules.Openddict.Commands;
using OroIdentityServer.Application.Modules.Openddict.Queries;

namespace OroIdentityServer.Server.Endpoints;

public static partial class AdminApiEndpoints
{
    private static void MapOpenIddictScopes(this RouteGroupBuilder api)
    {
        var g = api.MapGroup("/scopes");

        g.MapGet("/", async ([FromServices] IQueryHandler<GetScopesQuery, IEnumerable<OpenIddictScopeDescriptor>> h, CancellationToken ct)
            => Results.Ok(await h.HandleAsync(new GetScopesQuery(), ct)));

        g.MapPost("/", async ([FromBody] CreateScopeCommand cmd, [FromServices] ICommandHandler<CreateScopeCommand> h, CancellationToken ct) =>
        {
            await h.HandleAsync(cmd, ct);
            return Results.Created("/api/scopes", null);
        });

        g.MapPut("/{name}", async (string name, [FromBody] UpdateScopeCommand cmd, [FromServices] ICommandHandler<UpdateScopeCommand> h, CancellationToken ct) =>
        {
            await h.HandleAsync(cmd with { Name = name }, ct);
            return Results.NoContent();
        });

        g.MapDelete("/{name}", async (string name, [FromServices] ICommandHandler<DeleteScopeCommand> h, CancellationToken ct) =>
        {
            await h.HandleAsync(new DeleteScopeCommand(name), ct);
            return Results.NoContent();
        });
    }
}
