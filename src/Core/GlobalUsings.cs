// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Hosting;
global using OroIdentityServer.Core.Interfaces;
global using OroIdentityServer.Core.Modules.IdentificationTypes.Aggregates;
global using OroIdentityServer.Core.Modules.IdentificationTypes.DomainEvents;
global using OroIdentityServer.Core.Modules.IdentificationTypes.ValueObjects;
global using OroIdentityServer.Core.Modules.Permissions.Aggregates;
global using OroIdentityServer.Core.Modules.Permissions.DoaminEvents;
global using OroIdentityServer.Core.Modules.Roles.Aggregates;
global using OroIdentityServer.Core.Modules.Roles.DomainEvents;
global using OroIdentityServer.Core.Modules.Tenants.Aggregates;
global using OroIdentityServer.Core.Modules.Tenants.DomainEvents;
global using OroIdentityServer.Core.Modules.Tenants.ValueObjects;
global using OroIdentityServer.Core.Modules.Users.Aggregates;
global using OroIdentityServer.Core.Modules.Users.DomainEvents;
global using OroIdentityServer.Core.Modules.Users.Entities;
global using OroIdentityServer.Core.Modules.Users.ValueObjects;
global using OroIdentityServer.Core.Modules.UserSessions.Aggregates;
global using OroIdentityServer.Core.Modules.UserSessions.DomainEvents;
global using OroIdentityServer.Core.Modules.UserSessions.Entities;
global using OroIdentityServer.Core.Modules.UserSessions.ValueObjects;
global using OroIdentityServer.Core.Services;
global using OroIdentityServer.Core.Shared;
global using OroKernel.Shared.Entities;
global using OroKernel.Shared.Events;
global using OroKernel.Shared.Interfaces;

global using OroIdentityServer.Core.Modules.UserPreferences.ValueObjects;
global using OroIdentityServer.Core.Modules.UserPreferences.Entities;
global using OroIdentityServer.Core.Modules.UserPreferences.Aggregates;
global using OroIdentityServer.Core.Modules.UserPreferences.Events;
global using System.Text.RegularExpressions;
global using OroIdentityServer.Core.Modules.Roles.Entities;
global using OroKernel.Shared.Exceptions;
