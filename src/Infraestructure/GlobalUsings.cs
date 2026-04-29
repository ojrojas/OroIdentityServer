// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
global using System.Linq.Expressions;
global using System.Text.Json;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.EntityFrameworkCore.Metadata.Builders;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Hosting;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Options;
global using OpenIddict.Abstractions;
global using OroIdentityServer.Core.Interfaces;
global using OroIdentityServer.Core.Modules.IdentificationTypes.Aggregates;
global using OroIdentityServer.Core.Modules.IdentificationTypes.Repositories;
global using OroIdentityServer.Core.Modules.Permissions.Aggregates;
global using OroIdentityServer.Core.Modules.Permissions.Repositories;
global using OroIdentityServer.Core.Modules.Roles.Aggregates;
global using OroIdentityServer.Core.Modules.Roles.Entities;
global using OroIdentityServer.Core.Modules.Roles.Repositories;
global using OroIdentityServer.Core.Modules.Tenants.Aggregates;
global using OroIdentityServer.Core.Modules.Tenants.Repositories;
global using OroIdentityServer.Core.Modules.Tenants.ValueObjects;
global using OroIdentityServer.Core.Modules.UserPreferences.Aggregates;
global using OroIdentityServer.Core.Modules.UserPreferences.Entities;
global using OroIdentityServer.Core.Modules.UserPreferences.ValueObjects;
global using OroIdentityServer.Core.Modules.Users.Aggregates;
global using OroIdentityServer.Core.Modules.Users.Entities;
global using OroIdentityServer.Core.Modules.Users.Repositories;
global using OroIdentityServer.Core.Modules.UserSessions.Aggregates;
global using OroIdentityServer.Core.Modules.UserSessions.Entities;
global using OroIdentityServer.Core.Modules.UserSessions.Repositories;
global using OroIdentityServer.Core.Modules.UserSessions.ValueObjects;
global using OroIdentityServer.Core.Shared;
global using OroIdentityServer.Infraestructure;
global using OroIdentityServer.Infraestructure.Data.Configurations;
global using OroIdentityServer.OroIdentityServer.Infraestructure.Interfaces;
global using OroIdentityServer.OroIdentityServer.Infraestructure.Specifications;
global using OroKernel.Shared.Data;
global using OroKernel.Shared.Interfaces;
global using OroKernel.Shared.Options;
