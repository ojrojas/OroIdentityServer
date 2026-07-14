using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OroCQRS.Core.Interfaces;
using OroIdentityServer.Application.Modules.Openddict.Commands;
using OroIdentityServer.Application.Modules.Openddict.Queries;
using OroIdentityServer.Application.Shared;

namespace OroIdentityServer.Server.Endpoints;

public static partial class AdminApiEndpoints
{
    private static void MapOpenIddictApplications(this RouteGroupBuilder api)
    {
        var g = api.MapGroup("/applications");

        g.MapGet("/", async ([FromServices] IQueryHandler<GetApplicationsQuery, IEnumerable<OpenIddictApplicationDescriptor>> h, CancellationToken ct)
            => Results.Ok(await h.HandleAsync(new GetApplicationsQuery(), ct)));

        g.MapGet("/{clientId}", async (string clientId, [FromServices] IQueryHandler<GetApplicationByClientIdQuery, OpenIddictApplicationDescriptor> h, CancellationToken ct)
            => Results.Ok(await h.HandleAsync(new GetApplicationByClientIdQuery(clientId), ct)));

        g.MapPost("/", async ([FromBody] OpenIddictApplicationDescriptor descriptor, [FromServices] ICommandHandler<CreateApplicationCommand> h, CancellationToken ct) =>
        {
            await h.HandleAsync(new CreateApplicationCommand(MapToApplicationDescriptor(descriptor)), ct);
            return Results.Created($"/api/applications/{descriptor.ClientId}", null);
        });

        g.MapPut("/{clientId}", async (string clientId, [FromBody] OpenIddictApplicationDescriptor descriptor, [FromServices] ICommandHandler<UpdateApplicationCommand> h, CancellationToken ct) =>
        {
            descriptor.ClientId = clientId;
            await h.HandleAsync(new UpdateApplicationCommand(MapToApplicationDescriptor(descriptor)), ct);
            return Results.NoContent();
        });

        g.MapDelete("/{clientId}", async (string clientId, [FromServices] ICommandHandler<DeleteApplicationCommand> h, CancellationToken ct) =>
        {
            await h.HandleAsync(new DeleteApplicationCommand(clientId), ct);
            return Results.NoContent();
        });
    }

    private static ApplicationDescriptor MapToApplicationDescriptor(OpenIddictApplicationDescriptor source)
    {
        var dest = new ApplicationDescriptor
        {
            ClientId = source.ClientId,
            DisplayName = source.DisplayName,
            ClientSecret = source.ClientSecret,
            ClientType = source.ClientType,
            ApplicationType = source.ApplicationType,
            ConsentType = source.ConsentType
        };

        foreach (var p in source.Permissions) dest.Permissions.Add(p);
        foreach (var r in source.Requirements) dest.Requirements.Add(r);
        foreach (var u in source.RedirectUris) dest.RedirectUris.Add(u);
        foreach (var u in source.PostLogoutRedirectUris) dest.PostLogoutRedirectUris.Add(u);

        return dest;
    }
}
