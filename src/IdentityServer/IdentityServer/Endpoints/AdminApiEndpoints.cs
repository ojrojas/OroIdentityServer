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
            .RequireAuthorization("ManagerOrAdmin")
            .WithTags("Admin API");

        api.MapUsers();
        api.MapUserMfa();
        api.MapRoles();
        api.MapPermissions();
        api.MapTenants();
        api.MapIdentificationTypes();
        api.MapUserSessions();
        api.MapSessions();
        api.MapValidationLogs();
        api.MapHierarchy();
        app.MapDashboard();
        api.MapOpenIddictApplications();
        api.MapOpenIddictScopes();

        // Self-service MFA (/api/me/*) is registered on the app, not on the ManagerOrAdmin group.
        app.MapSelfServiceMfa();

        return app;
    }

    /// <summary>
    /// ServerAdminXxxService write methods return HttpResponseMessage (so the same IAdminXxxService
    /// interface works for both the HTTP-based client and this CQRS-based server implementation).
    /// This translates that back into a minimal API IResult.
    /// </summary>
    private static async Task<IResult> ToResultAsync(HttpResponseMessage response, CancellationToken ct)
    {
        // Read the body unconditionally instead of trusting ContentLength: JsonContent is sent
        // with chunked transfer encoding, so the header is absent and the payload would be
        // replaced by the status-only fallback below.
        var body = response.Content is null
            ? string.Empty
            : await response.Content.ReadAsStringAsync(ct);

        if (!string.IsNullOrEmpty(body))
            return Results.Content(body, "application/json", statusCode: (int)response.StatusCode);

        // Write a JSON body so the status code pages middleware does not re-execute the request
        // against /not-found (which only handles GET) - that re-execution turns POST/DELETE failures
        // into 400/405 responses.
        return Results.Json(new { status = (int)response.StatusCode }, statusCode: (int)response.StatusCode);
    }
}
