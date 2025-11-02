// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
global using OroIdentityServer.BuildingBlocks.Loggers;
global using OroIdentityServer.Services.OroIdentityServer.Infraestructure;
global using Microsoft.AspNetCore.Authorization;
global using Microsoft.AspNetCore.Http.HttpResults;
global using OpenIddict.Validation.AspNetCore;
global using OroIdentityServer.Services.OroIdentityServer.Application.Queries;
global using OroCQRS.Core.Interfaces;
global using Microsoft.AspNetCore.Mvc;
global using OroIdentityServer.OroIdentityServer.Infraestructure.Repositories.Extensions;
global using OroIdentityServer.Services.OroIdentityServer.Api.Endpoints;
global using OroIdentityServer.Services.OroIdentityServer.Application.Extensions;
global using OroIdentityServer.Services.OroIdentityServer.Core.Extensions;
global using Serilog;
global using OroIdentityServer.Services.OroIdentityServer.Application.Commands;
global using Microsoft.AspNetCore.Routing.Constraints;
