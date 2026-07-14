// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using OroIdentityServer.Application.Shared;

namespace OroIdentityServer.Server.Endpoints;

public static class AdminApiEndpoints
{
    public static IEndpointRouteBuilder MapAdminApiEndpoints(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api")
            .RequireAuthorization("AdminOnly")
            .WithTags("Admin API");

        api.MapUsers();
        api.MapRoles();
        api.MapPermissions();
        api.MapTenants();
        api.MapIdentificationTypes();
        api.MapUserSessions();
        api.MapSessions();
        api.MapOpenIddictApplications();
        api.MapOpenIddictScopes();

        return app;
    }

    // ---------------- Users ----------------
    private static void MapUsers(this RouteGroupBuilder api)
    {
        var g = api.MapGroup("/users");

        g.MapGet("/", async (IQueryHandler<GetUsersQuery, GetUsersQueryResponse> h, CancellationToken ct)
            => Results.Ok(await h.HandleAsync(new GetUsersQuery(), ct)));

        g.MapPost("/", async (
            CreateUserCommand cmd,
            ICommandHandler<CreateUserCommand> h,
            CancellationToken ct) =>
        {
            await h.HandleAsync(cmd, ct);
            return Results.Created($"/api/users", null);
        });

        g.MapPut("/{id:guid}", async (
            Guid id,
            UpdateUserCommand cmd,
            ICommandHandler<UpdateUserCommand, UpdateUserResponse> h,
            CancellationToken ct) =>
        {
            var result = await h.HandleAsync(cmd with { UserId = id }, ct);
            return Results.Ok(result);
        });

        g.MapDelete("/{id:guid}", async (
            Guid id,
            ICommandHandler<DeleteUserCommand> h,
            CancellationToken ct) =>
        {
            await h.HandleAsync(new DeleteUserCommand(id), ct);
            return Results.NoContent();
        });
    }

    // ---------------- Roles ----------------
    private static void MapRoles(this RouteGroupBuilder api)
    {
        var g = api.MapGroup("/roles");

        g.MapGet("/", async (IQueryHandler<GetRolesQuery, GetRolesResponse> h, CancellationToken ct)
            => Results.Ok(await h.HandleAsync(new GetRolesQuery(), ct)));

        g.MapGet("/{id:guid}", async (Guid id, IQueryHandler<GetRoleByIdQuery, GetRoleByIdResponse> h, CancellationToken ct)
            => Results.Ok(await h.HandleAsync(new GetRoleByIdQuery(id), ct)));

        g.MapPost("/", async (CreateRoleCommand cmd, ICommandHandler<CreateRoleCommand> h, CancellationToken ct) =>
        {
            await h.HandleAsync(cmd, ct);
            return Results.Created("/api/roles", null);
        });

        g.MapPut("/{id:guid}", async (Guid id, UpdateRoleCommand cmd, ICommandHandler<UpdateRoleCommand> h, CancellationToken ct) =>
        {
            await h.HandleAsync(cmd with { Id = id }, ct);
            return Results.NoContent();
        });

        g.MapDelete("/{id:guid}", async (Guid id, ICommandHandler<DeleteRoleCommand> h, CancellationToken ct) =>
        {
            await h.HandleAsync(new DeleteRoleCommand(id), ct);
            return Results.NoContent();
        });
    }

    // ---------------- Permissions ----------------
    private static void MapPermissions(this RouteGroupBuilder api)
    {
        var g = api.MapGroup("/permissions");

        g.MapGet("/", async (IQueryHandler<GetPermissionsQuery, GetPermissionsQueryResponse> h, CancellationToken ct)
            => Results.Ok(await h.HandleAsync(new GetPermissionsQuery(), ct)));

        g.MapGet("/{id:guid}", async (Guid id, IQueryHandler<GetPermissionByIdQuery, GetPermissionByIdQueryResponse> h, CancellationToken ct)
            => Results.Ok(await h.HandleAsync(new GetPermissionByIdQuery(id), ct)));

        g.MapPost("/", async (CreatePermissionCommand cmd, ICommandHandler<CreatePermissionCommand> h, CancellationToken ct) =>
        {
            await h.HandleAsync(cmd, ct);
            return Results.Created("/api/permissions", null);
        });

        g.MapPut("/{id:guid}", async (Guid id, UpdatePermissionCommand cmd, ICommandHandler<UpdatePermissionCommand> h, CancellationToken ct) =>
        {
            await h.HandleAsync(cmd with { PermissionId = id }, ct);
            return Results.NoContent();
        });

        g.MapDelete("/{id:guid}", async (Guid id, ICommandHandler<DeletePermissionCommand> h, CancellationToken ct) =>
        {
            await h.HandleAsync(new DeletePermissionCommand(id), ct);
            return Results.NoContent();
        });
    }

    // ---------------- Tenants ----------------
    private static void MapTenants(this RouteGroupBuilder api)
    {
        var g = api.MapGroup("/tenants");

        g.MapGet("/", async (IQueryHandler<GetTenantsQuery, GetTenantsResponse> h, CancellationToken ct)
            => Results.Ok(await h.HandleAsync(new GetTenantsQuery(), ct)));

        g.MapGet("/{id:guid}", async (Guid id, IQueryHandler<GetTenantByIdQuery, GetTenantByIdResponse> h, CancellationToken ct)
            => Results.Ok(await h.HandleAsync(new GetTenantByIdQuery(id), ct)));

        g.MapGet("/by-user/{userId:guid}", async (Guid userId, IQueryHandler<GetTenantsByUserIdQuery, GetTenantsByUserIdResponse> h, CancellationToken ct)
            => Results.Ok(await h.HandleAsync(new GetTenantsByUserIdQuery(userId), ct)));

        g.MapPost("/", async (CreateTenantCommand cmd, ICommandHandler<CreateTenantCommand> h, CancellationToken ct) =>
        {
            await h.HandleAsync(cmd, ct);
            return Results.Created("/api/tenants", null);
        });

        g.MapPut("/{id:guid}", async (Guid id, UpdateTenantCommand cmd, ICommandHandler<UpdateTenantCommand> h, CancellationToken ct) =>
        {
            await h.HandleAsync(cmd with { TenantId = id }, ct);
            return Results.NoContent();
        });

        g.MapPost("/{id:guid}/activate", async (Guid id, ICommandHandler<ActivateTenantCommand> h, CancellationToken ct) =>
        {
            await h.HandleAsync(new ActivateTenantCommand(id), ct);
            return Results.NoContent();
        });

        g.MapPost("/{id:guid}/suspend", async (Guid id, ICommandHandler<SuspendTenantCommand> h, CancellationToken ct) =>
        {
            await h.HandleAsync(new SuspendTenantCommand(id), ct);
            return Results.NoContent();
        });

        g.MapPost("/{id:guid}/users", async (Guid id, AddTenantUserCommand cmd, ICommandHandler<AddTenantUserCommand> h, CancellationToken ct) =>
        {
            await h.HandleAsync(cmd with { TenantId = id }, ct);
            return Results.NoContent();
        });
    }

    // ---------------- IdentificationTypes ----------------
    private static void MapIdentificationTypes(this RouteGroupBuilder api)
    {
        var g = api.MapGroup("/identification-types");

        g.MapGet("/", async (IQueryHandler<GetIdentificationTypesQuery, GetIdentificationTypesResponse> h, CancellationToken ct)
            => Results.Ok(await h.HandleAsync(new GetIdentificationTypesQuery(), ct)));

        g.MapGet("/{id:guid}", async (Guid id, IQueryHandler<GetIdentificationTypeByIdQuery, GetIdentificationTypeByIdResponse> h, CancellationToken ct)
            => Results.Ok(await h.HandleAsync(new GetIdentificationTypeByIdQuery(id), ct)));

        g.MapPost("/", async (CreateIdentificationTypeCommand cmd, ICommandHandler<CreateIdentificationTypeCommand> h, CancellationToken ct) =>
        {
            await h.HandleAsync(cmd, ct);
            return Results.Created("/api/identification-types", null);
        });

        g.MapPut("/{id:guid}", async (Guid id, UpdateIdentificationTypeCommand cmd, ICommandHandler<UpdateIdentificationTypeCommand> h, CancellationToken ct) =>
        {
            await h.HandleAsync(cmd with { Id = id }, ct);
            return Results.NoContent();
        });

        g.MapDelete("/{id:guid}", async (Guid id, ICommandHandler<DeleteIdentificationTypeCommand> h, CancellationToken ct) =>
        {
            await h.HandleAsync(new DeleteIdentificationTypeCommand(id), ct);
            return Results.NoContent();
        });
    }

    // ---------------- UserSessions ----------------
    private static void MapUserSessions(this RouteGroupBuilder api)
    {
        var g = api.MapGroup("/user-sessions");

        g.MapGet("/by-user/{userId:guid}", async (Guid userId, IQueryHandler<GetUserSessionsByUserQuery, IEnumerable<OroIdentityServer.Core.Modules.UserSessions.Aggregates.UserSession>> h, CancellationToken ct)
            => Results.Ok(await h.HandleAsync(new GetUserSessionsByUserQuery(userId), ct)));

        g.MapPost("/", async (CreateUserSessionCommand cmd, ICommandHandler<CreateUserSessionCommand> h, CancellationToken ct) =>
        {
            await h.HandleAsync(cmd, ct);
            return Results.Created("/api/user-sessions", null);
        });

        g.MapPost("/{id:guid}/deactivate", async (Guid id, ICommandHandler<DeactivateUserSessionCommand> h, CancellationToken ct) =>
        {
            await h.HandleAsync(new DeactivateUserSessionCommand(id), ct);
            return Results.NoContent();
        });
    }

    // ---------------- Sessions ----------------
    private static void MapSessions(this RouteGroupBuilder api)
    {
        var g = api.MapGroup("/sessions");

        g.MapGet("/by-user/{userId:guid}", async (Guid userId, IQueryHandler<GetUserSessionsQuery, GetUserSessionsQueryResponse> h, CancellationToken ct)
            => Results.Ok(await h.HandleAsync(new GetUserSessionsQuery(userId), ct)));
    }

    // ---------------- OpenIddict Applications ----------------
    private static void MapOpenIddictApplications(this RouteGroupBuilder api)
    {
        var g = api.MapGroup("/applications");

        g.MapGet("/", async (IQueryHandler<GetApplicationsQuery, IEnumerable<OpenIddictApplicationDescriptor>> h, CancellationToken ct)
            => Results.Ok(await h.HandleAsync(new GetApplicationsQuery(), ct)));

        g.MapGet("/{clientId}", async (string clientId, IQueryHandler<GetApplicationByClientIdQuery, OpenIddictApplicationDescriptor> h, CancellationToken ct)
            => Results.Ok(await h.HandleAsync(new GetApplicationByClientIdQuery(clientId), ct)));

        g.MapPost("/", async (OpenIddictApplicationDescriptor descriptor, ICommandHandler<CreateApplicationCommand> h, CancellationToken ct) =>
        {
            await h.HandleAsync(new CreateApplicationCommand(MapToApplicationDescriptor(descriptor)), ct);
            return Results.Created($"/api/applications/{descriptor.ClientId}", null);
        });

        g.MapPut("/{clientId}", async (string clientId, OpenIddictApplicationDescriptor descriptor, ICommandHandler<UpdateApplicationCommand> h, CancellationToken ct) =>
        {
            descriptor.ClientId = clientId;
            await h.HandleAsync(new UpdateApplicationCommand(MapToApplicationDescriptor(descriptor)), ct);
            return Results.NoContent();
        });

        g.MapDelete("/{clientId}", async (string clientId, ICommandHandler<DeleteApplicationCommand> h, CancellationToken ct) =>
        {
            await h.HandleAsync(new DeleteApplicationCommand(clientId), ct);
            return Results.NoContent();
        });
    }

    // ---------------- OpenIddict Scopes ----------------
    private static void MapOpenIddictScopes(this RouteGroupBuilder api)
    {
        var g = api.MapGroup("/scopes");

        g.MapGet("/", async (IQueryHandler<GetScopesQuery, IEnumerable<OpenIddictScopeDescriptor>> h, CancellationToken ct)
            => Results.Ok(await h.HandleAsync(new GetScopesQuery(), ct)));

        g.MapPost("/", async (CreateScopeCommand cmd, ICommandHandler<CreateScopeCommand> h, CancellationToken ct) =>
        {
            await h.HandleAsync(cmd, ct);
            return Results.Created("/api/scopes", null);
        });

        g.MapPut("/{name}", async (string name, UpdateScopeCommand cmd, ICommandHandler<UpdateScopeCommand> h, CancellationToken ct) =>
        {
            await h.HandleAsync(cmd with { Name = name }, ct);
            return Results.NoContent();
        });

        g.MapDelete("/{name}", async (string name, ICommandHandler<DeleteScopeCommand> h, CancellationToken ct) =>
        {
            await h.HandleAsync(new DeleteScopeCommand(name), ct);
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
