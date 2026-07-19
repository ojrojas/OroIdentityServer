using Microsoft.AspNetCore.Mvc;
using IdentityServer.Client.Models.IdentificationTypes;
using IdentityServer.Client.Interfaces;

namespace OroIdentityServer.Server.Endpoints;

public static partial class AdminApiEndpoints
{
    private static void MapIdentificationTypes(this RouteGroupBuilder api)
    {
        var g = api.MapGroup("/identification-types");

        g.MapGet("/", async ([FromServices] IAdminIdentificationTypeService service, CancellationToken ct)
            => Results.Ok(await service.GetIdentificationTypesAsync(ct)));

        g.MapGet("/{id:guid}", async (Guid id, [FromServices] IAdminIdentificationTypeService service, CancellationToken ct)
            => Results.Ok(await service.GetIdentificationTypeByIdAsync(id, ct)));

        g.MapPost("/", async ([FromBody] CreateIdentificationTypeRequest request, [FromServices] IAdminIdentificationTypeService service, CancellationToken ct)
            => await ToResultAsync(await service.CreateIdentificationTypeAsync(request, ct), ct));

        g.MapPut("/{id:guid}", async (Guid id, [FromBody] UpdateIdentificationTypeRequest request, [FromServices] IAdminIdentificationTypeService service, CancellationToken ct)
            => await ToResultAsync(await service.UpdateIdentificationTypeAsync(id, request, ct), ct));

        g.MapDelete("/{id:guid}", async (Guid id, [FromServices] IAdminIdentificationTypeService service, CancellationToken ct)
            => await ToResultAsync(await service.DeleteIdentificationTypeAsync(id, ct), ct));
    }
}
