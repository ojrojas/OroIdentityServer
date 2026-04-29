// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
global using System.Globalization;
global using System.Net;
global using System.Text.Json;
global using FluentValidation;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.Hosting;
global using Microsoft.Extensions.Logging;
global using Microsoft.IdentityModel.Tokens;
global using OpenIddict.Abstractions;
global using OroBuildingBlocks.EventBus.Abstractions;
global using OroBuildingBlocks.EventBus.Events;
global using OroCQRS.Core.Extensions;
global using OroCQRS.Core.Interfaces;
global using OroIdentityServer.Application.Abstractions.Mappers;
global using OroIdentityServer.Application.Modules.Permissions.DTOs;
global using OroIdentityServer.Application.Modules.Tenants.DTOs;
global using OroIdentityServer.Application.Shared;
global using OroIdentityServer.Core.Interfaces;
global using OroIdentityServer.Core.Modules.IdentificationTypes.Aggregates;
global using OroIdentityServer.Core.Modules.IdentificationTypes.Repositories;
global using OroIdentityServer.Core.Modules.Permissions.Aggregates;
global using OroIdentityServer.Core.Modules.Permissions.Repositories;
global using OroIdentityServer.Core.Modules.Roles.Aggregates;
global using OroIdentityServer.Core.Modules.Roles.Repositories;
global using OroIdentityServer.Core.Modules.Tenants.Repositories;
global using OroIdentityServer.Core.Modules.Tenants.ValueObjects;
global using OroIdentityServer.Core.Modules.Users.Aggregates;
global using OroIdentityServer.Core.Modules.Users.Entities;
global using OroIdentityServer.Core.Modules.Users.Repositories;
global using OroIdentityServer.Core.Modules.UserSessions.Aggregates;
global using OroIdentityServer.Core.Modules.UserSessions.Entities;
global using OroIdentityServer.Core.Modules.UserSessions.Repositories;
global using OroIdentityServer.Core.Shared;
global using OroIdentityServer.OroIdentityServer.Infraestructure.Interfaces;
global using OroKernel.Shared.Enums;
global using OroIdentityServer.Application.Modules.Tenants.Commands;

global using OroIdentityServer.Core.Modules.Roles.DomainEvents;
global using OroIdentityServer.Application.Modules.Roles.DTOs;
global using OroIdentityServer.Application.Modules.IdentificationTypes.DTOs;
global using OroIdentityServer.Application.Modules.Tenants.IntegrationEvents;
global using OroIdentityServer.Core.Modules.Tenants.Aggregates;
global using OroIdentityServer.Application.Modules.Tenants.Interfaces;
global using OroIdentityServer.Application.Modules.Sessions.DTOs;
global using OroIdentityServer.Application.Modules.Users.DTOs;
global using OroBuildingBlocks.EventBusRabbitMQ;