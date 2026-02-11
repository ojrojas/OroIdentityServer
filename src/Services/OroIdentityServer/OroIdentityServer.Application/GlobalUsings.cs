// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
global using OroCQRS.Core.Interfaces;
global using OroIdentityServer.Services.OroIdentityServer.Core.Interfaces;
global using OroIdentityServer.Services.OroIdentityServer.Core.Models;
global using Microsoft.Extensions.Logging;
global using OroIdentityServer.Services.OroIdentityServer.Application.Shared;
global using Microsoft.Extensions.Hosting;
global using Microsoft.Extensions.Configuration;
global using OroCQRS.Core.Extensions;
global using OpenIddict.Abstractions;
global using OroKernel.Shared.Enums;
global using OroIdentityServer.OroIdentityServer.Infraestructure.Interfaces;
global using OroIdentityServer.Services.OroIdentityServer.Application.Queries;
global using System.Globalization;
global using System.Text.Json;
global using Microsoft.IdentityModel.Tokens;