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
global using OroIdentityServer.OroIdentityServer.Infraestructure.Repositories.Interfaces;
global using OroIdentityServer.OroIdentityServer.Infraestructure.Specifications;
global using OroIdentityServer.Services.OroIdentityServer.Core.Interfaces;
global using OroIdentityServer.Services.OroIdentityServer.Core.Models;
global using OroIdentityServer.Services.OroIdentityServer.Infraestructure;
global using OroKernel.Shared.Options;
global using OroKernel.Shared.Interfaces;
global using OroKernel.Shared.Events;


global using OroKernel.Shared.Data;