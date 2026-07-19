using Microsoft.AspNetCore.Mvc;
using IdentityServer.Client.Models.OpenIddict;
using IdentityServer.Client.Interfaces;

namespace OroIdentityServer.Server.Endpoints;

public static partial class AdminApiEndpoints
{
    private static void MapOpenIddictScopes(this RouteGroupBuilder api)
    {
        var g = api.MapGroup("/scopes");

        g.MapGet("/", async ([FromServices] IAdminScopeService service, CancellationToken ct)
            => Results.Ok(await service.GetScopesAsync(ct)));

        g.MapPost("/", async ([FromBody] CreateOpenIddictScopeRequest request, [FromServices] IAdminScopeService service, CancellationToken ct)
            => await ToResultAsync(await service.CreateScopeAsync(request, ct), ct));

        g.MapPut("/{name}", async (string name, [FromBody] UpdateOpenIddictScopeRequest request, [FromServices] IAdminScopeService service, CancellationToken ct)
            => await ToResultAsync(await service.UpdateScopeAsync(name, request, ct), ct));

        g.MapDelete("/{name}", async (string name, [FromServices] IAdminScopeService service, CancellationToken ct)
            => await ToResultAsync(await service.DeleteScopeAsync(name, ct), ct));
    }
}
