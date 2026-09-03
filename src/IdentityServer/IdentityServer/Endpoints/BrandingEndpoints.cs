// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
using IdentityServer.Client.Interfaces;

namespace OroIdentityServer.Server.Endpoints;

public static class BrandingEndpoints
{
    public static void MapBranding(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/branding", async (IAdminBrandingService service) => Results.Ok(await service.GetBrandingOptionsAsync(default)))
            .AllowAnonymous();
    }
}
