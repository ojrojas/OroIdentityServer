// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using OroIdentity.Web.Client.Interfaces;
using OroIdentity.Web.Server.Handlers;
using OroIdentity.Web.Server.Services;

namespace OroIdentity.Web.Server.Extensiones;

public static class AppHttpClientExtensions
{
    public static TBuilder AddAppHttpClientExtensions<TBuilder>(
       this TBuilder builder, string identityUri) where TBuilder : IHostApplicationBuilder
    {

        builder.Services.AddHttpClient<IApplicationsService, ApplicationsService>(
            client =>
            {
                client.BaseAddress = new Uri(identityUri) ??
                throw new Exception("Missing base address environment");
            }
        ).AddHttpMessageHandler<TokenHandler>();

        builder.Services.AddHttpClient<IRolesService, RolesService>(
            client =>
            {
                client.BaseAddress = new Uri(identityUri) ??
                throw new Exception("Missing base address environment");
            }
        ).AddHttpMessageHandler<TokenHandler>();

        builder.Services.AddHttpClient<IScopesService, ScopesService>(
            client =>
            {
                client.BaseAddress = new Uri(identityUri) ??
                throw new Exception("Missing base address environment");
            }
        ).AddHttpMessageHandler<TokenHandler>();

        builder.Services.AddHttpClient<IUsersService, UsersService>(
            client =>
            {
                client.BaseAddress = new Uri(identityUri) ??
                throw new Exception("Missing base address environment");
            }
        ).AddHttpMessageHandler<TokenHandler>();

        builder.Services.AddHttpClient<IIdentificationTypeService, IdentificationTypesService>(
            client =>
            {
                client.BaseAddress = new Uri(identityUri) ??
                throw new Exception("Missing base address environment");
            }
        ).AddHttpMessageHandler<TokenHandler>();

        builder.Services.AddHttpClient<IPermissionsService, PermissionsService>(
            client =>
            {
                client.BaseAddress = new Uri(identityUri) ??
                throw new Exception("Missing base address environment");
            }
        ).AddHttpMessageHandler<TokenHandler>();

        builder.Services.AddHttpClient<ITenantsService, TenantsService>(
            client =>
            {
                client.BaseAddress = new Uri(identityUri) ??
                throw new Exception("Missing base address environment");
            }
        ).AddHttpMessageHandler<TokenHandler>();

        // Sessions proxy service to aggregate and manage sessions from identity server
        builder.Services.AddHttpClient<ISessionsService, SessionsService>(
            client =>
            {
                client.BaseAddress = new Uri(identityUri) ??
                throw new Exception("Missing base address environment");
            }
        ).AddHttpMessageHandler<TokenHandler>();

        builder.Services.AddHttpClient<IRoleClaimsService, RoleClaimsService>(
            client =>
            {
                client.BaseAddress = new Uri(identityUri) ??
                throw new Exception("Missing base address environment");
            }
        ).AddHttpMessageHandler<TokenHandler>();

        return builder;
    }
}