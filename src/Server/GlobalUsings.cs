// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
global using System.Collections;
global using System.Diagnostics.CodeAnalysis;
global using System.Reflection;
global using System.Security.Claims;
global using Microsoft.AspNetCore;
global using Microsoft.AspNetCore.Authentication;
global using Microsoft.AspNetCore.Authentication.Cookies;
global using Microsoft.AspNetCore.Authorization;
global using Microsoft.AspNetCore.Http.HttpResults;
global using Microsoft.AspNetCore.Identity;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.AspNetCore.Routing.Constraints;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.IdentityModel.Tokens;
global using OpenIddict.Abstractions;
global using OpenIddict.Server.AspNetCore;
global using OpenIddict.Validation.AspNetCore;
global using OroBuildingBlocks.Loggers;
global using OroCQRS.Core.Interfaces;
global using OroIdentityServer.Application.Extensions;
global using OroIdentityServer.Core.Extensions;
global using OroIdentityServer.Infraestructure;
global using OroIdentityServer.OroIdentityServer.Infraestructure.Data;
global using OroIdentityServer.OroIdentityServer.Infraestructure.Repositories.Extensions;
global using OroIdentityServer.Server.Adapters;
global using OroIdentityServer.Server.Endpoints;
global using OroIdentityServer.Server.Enums;
global using OroIdentityServer.Server.Extensions;
global using OroIdentityServer.Server.Models;
global using Quartz;
global using Scalar.AspNetCore;
global using Serilog;
global using static OpenIddict.Abstractions.OpenIddictConstants;
