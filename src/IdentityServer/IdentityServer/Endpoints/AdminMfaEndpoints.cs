// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using System.Net;
using BuildingBlocks.CQRS.Abstractions;
using IdentityServer.Client.Interfaces;
using IdentityServer.Client.Models.Mfa;
using Microsoft.AspNetCore.Mvc;
using OroIdentityServer.Application.Modules.Mfa.Commands;

namespace OroIdentityServer.Server.Endpoints;

public static partial class AdminApiEndpoints
{
    /// <summary>
    /// Self-service MFA. This lives outside the /api group on purpose: that group requires
    /// ManagerOrAdmin, and a plain member must still be able to manage their own second factor.
    /// Only the CQRS-backed MFA service decides whose factor is affected, and it takes
    /// the user id from the session, never from the request body.
    /// </summary>
    private static void MapSelfServiceMfa(this IEndpointRouteBuilder app)
    {
        var me = app.MapGroup("/api/me")
            .RequireAuthorization()
            .WithTags("MFA");

        me.MapGet("/mfa", async ([FromServices] IAdminMfaService service, CancellationToken ct)
            => Results.Ok(await service.GetStatusAsync(ct)));

        me.MapPost("/mfa/setup", async ([FromServices] IAdminMfaService service, CancellationToken ct)
            => await ToResultAsync(await service.StartEnrollmentAsync(ct), ct));

        me.MapPost("/mfa/confirm", async ([FromBody] MfaConfirmRequest request, [FromServices] IAdminMfaService service, CancellationToken ct)
            => await ToResultAsync(await service.ConfirmEnrollmentAsync(request.Code, ct), ct));

        me.MapPost("/mfa/disable", async ([FromBody] MfaDisableRequest request, [FromServices] IAdminMfaService service, CancellationToken ct)
            => await ToResultAsync(await service.DisableAsync(request, ct), ct));

        me.MapPost("/mfa/recovery-codes", async ([FromServices] IAdminMfaService service, CancellationToken ct)
            => await ToResultAsync(await service.RegenerateRecoveryCodesAsync(ct), ct));
    }

    /// <summary>Administrative reset of another user's factor (ManagerOrAdmin).</summary>
    private static void MapUserMfa(this RouteGroupBuilder api)
    {
        api.MapPost("/users/{id:guid}/mfa/reset", async (
            Guid id,
            [FromServices] ICommandDispatcher commandDispatcher,
            CancellationToken ct) =>
        {
            var result = await commandDispatcher.SendAsync(new ResetMfaCommand(id), ct);
            if (result.IsSuccess) return Results.NoContent();

            return Results.Json(new { status = (int)HttpStatusCode.BadRequest }, statusCode: (int)HttpStatusCode.BadRequest);
        });
    }
}