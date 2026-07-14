// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Server.Endpoints;

public static partial class AdminApiEndpoints
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
}
