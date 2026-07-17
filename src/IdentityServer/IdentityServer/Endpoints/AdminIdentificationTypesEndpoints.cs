using Microsoft.AspNetCore.Mvc;
using BuildingBlocks.CQRS.Abstractions;
using OroIdentityServer.Application.Modules.IdentificationTypes.Commands;
using OroIdentityServer.Application.Modules.IdentificationTypes.Queries;

namespace OroIdentityServer.Server.Endpoints;

public static partial class AdminApiEndpoints
{
    private static void MapIdentificationTypes(this RouteGroupBuilder api)
    {
        var g = api.MapGroup("/identification-types");

        g.MapGet("/", async ([FromServices] IQueryHandler<GetIdentificationTypesQuery, GetIdentificationTypesResponse> h, CancellationToken ct)
            => Results.Ok(await h.HandleAsync(new GetIdentificationTypesQuery(), ct)));

        g.MapGet("/{id:guid}", async (Guid id, [FromServices] IQueryHandler<GetIdentificationTypeByIdQuery, GetIdentificationTypeByIdResponse> h, CancellationToken ct)
            => Results.Ok(await h.HandleAsync(new GetIdentificationTypeByIdQuery(id), ct)));

        g.MapPost("/", async ([FromBody] CreateIdentificationTypeCommand cmd, [FromServices] ICommandHandler<CreateIdentificationTypeCommand> h, CancellationToken ct) =>
        {
            await h.HandleAsync(cmd, ct);
            return Results.Created("/api/identification-types", null);
        });

        g.MapPut("/{id:guid}", async (Guid id, [FromBody] UpdateIdentificationTypeCommand cmd, [FromServices] ICommandHandler<UpdateIdentificationTypeCommand> h, CancellationToken ct) =>
        {
            await h.HandleAsync(cmd with { Id = id }, ct);
            return Results.NoContent();
        });

        g.MapDelete("/{id:guid}", async (Guid id, [FromServices] ICommandHandler<DeleteIdentificationTypeCommand> h, CancellationToken ct) =>
        {
            await h.HandleAsync(new DeleteIdentificationTypeCommand(id), ct);
            return Results.NoContent();
        });
    }
}
