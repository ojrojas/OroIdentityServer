// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Services.OroIdentityServer.Api.Endpoints;

public static class RoleQueriesEndpoints
{
    extension(IEndpointRouteBuilder routeBuilder)
    {
        public RouteGroupBuilder MapRoleQueriesEndpointsV1()
        {
            var api = routeBuilder.MapGroup(string.Empty);

            return api;
        }
    }
}