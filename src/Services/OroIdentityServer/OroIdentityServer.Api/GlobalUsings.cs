// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
global using OroIdentityServer.BuildingBlocks.Loggers;
global using OroIdentityServer.Services.OroIdentityServer.Infraestructure;
global using Microsoft.AspNetCore.Authorization;
global using Microsoft.AspNetCore.Http.HttpResults;
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
global using OroIdentityServer.BuildingBlocks.ServiceDefaults;
global using Microsoft.IdentityModel.Tokens;
global using static OpenIddict.Abstractions.OpenIddictConstants;
global using OroIdentityServer.Services.OroIdentityServer.Api.Extensions;
global using Scalar.AspNetCore;
global using Quartz;
global using Microsoft.AspNetCore.Authentication.Cookies;
global using OpenIddict.Abstractions;
global using System.Security.Claims;
global using OpenIddict.Server.AspNetCore;
global using OroIdentityServer.Services.OroIdentityServer.Api.Enums;
global using Microsoft.AspNetCore.Authentication;
global using OroIdentityServer.Services.OroIdentityServer.Api.Models;