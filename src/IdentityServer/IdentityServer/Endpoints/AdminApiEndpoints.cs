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
        api.MapValidationLogs();
        api.MapOpenIddictApplications();
        api.MapOpenIddictScopes();

        return app;
    }

    /// <summary>
    /// ServerAdminXxxService write methods return HttpResponseMessage (so the same IAdminXxxService
    /// interface works for both the HTTP-based client and this CQRS-based server implementation).
    /// This translates that back into a minimal API IResult.
    /// </summary>
    private static async Task<IResult> ToResultAsync(HttpResponseMessage response, CancellationToken ct)
    {
        if (response.Content is { Headers.ContentLength: > 0 })
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            return Results.Content(body, "application/json", statusCode: (int)response.StatusCode);
        }

        return Results.StatusCode((int)response.StatusCode);
    }
}
