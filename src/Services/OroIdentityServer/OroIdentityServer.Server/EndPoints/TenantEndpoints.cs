// OroIdentityServer Server endpoints for Tenant management
namespace OroIdentityServer.Services.OroIdentityServer.Server.Endpoints;

public static class TenantEndpoints
{
    public static void MapTenantEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/tenants").RequireAuthorization();

        group.MapGet("/", async ([FromServices] ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new ListTenantsQuery(), cancellationToken);
            return Results.Ok(result);
        }).WithName("GetTenants");

        group.MapGet("/{id}", async (Guid id, [FromServices] ISender sender, CancellationToken cancellationToken) =>
        {
            var dto = await sender.Send(new GetTenantQuery(new TenantId(id)), cancellationToken);
            if (dto == null) return Results.NotFound();
            return Results.Ok(dto);
        }).WithName("GetTenant");

        group.MapPost("/", async ([FromBody] CreateTenantCommand cmd, [FromServices] ISender sender, CancellationToken cancellationToken) =>
        {
            await sender.Send(cmd, cancellationToken);
            return Results.Created($"/api/tenants/", null);
        }).WithName("CreateTenant");

        group.MapPut("/{id}", async (Guid id, [FromBody] UpdateTenantCommand cmd, [FromServices] ISender sender, CancellationToken cancellationToken) =>
        {
            if (id != cmd.Id.Value) return Results.BadRequest();
            await sender.Send(cmd, cancellationToken);
            return Results.NoContent();
        }).WithName("UpdateTenant");

        group.MapDelete("/{id}", async (Guid id, [FromServices] ISender sender, CancellationToken cancellationToken) =>
        {
            await sender.Send(new DeleteTenantCommand(new TenantId(id)), cancellationToken);
            return Results.NoContent();
        }).WithName("DeleteTenant");
    }
}
