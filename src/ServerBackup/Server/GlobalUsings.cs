// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
global using System.Security.Claims;
global using FluentValidation;
global using Microsoft.AspNetCore.Authentication;
global using Microsoft.AspNetCore.Authentication.Cookies;
global using Microsoft.AspNetCore.Builder;
global using Microsoft.AspNetCore.Http;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.AspNetCore.Routing;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;
global using OpenIddict.Abstractions;
global using OroCQRS.Core.Interfaces;
global using OroIdentityServer.Application.Modules.IdentificationTypes.Commands;
global using OroIdentityServer.Application.Modules.IdentificationTypes.Queries;
global using OroIdentityServer.Application.Modules.Openddict.Commands;
global using OroIdentityServer.Application.Modules.Openddict.Queries;
global using OroIdentityServer.Application.Modules.Permissions.Commands;
global using OroIdentityServer.Application.Modules.Permissions.Queries;
global using OroIdentityServer.Application.Modules.Roles.Commands;
global using OroIdentityServer.Application.Modules.Roles.Queries;
global using OroIdentityServer.Application.Modules.Sessions.Queries;
global using OroIdentityServer.Application.Modules.Tenants.Commands;
global using OroIdentityServer.Application.Modules.Tenants.Queries;
global using OroIdentityServer.Application.Modules.Users.Commands;
global using OroIdentityServer.Application.Modules.Users.Queries;
global using OroIdentityServer.Application.Modules.UserSessions.Commands;
global using OroIdentityServer.Application.Modules.UserSessions.Queries;
