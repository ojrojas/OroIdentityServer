// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
global using Microsoft.AspNetCore.Builder;
global using Microsoft.AspNetCore.Diagnostics.HealthChecks;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Diagnostics.HealthChecks;
global using Microsoft.Extensions.Logging;
global using OpenTelemetry;
global using OpenTelemetry.Metrics;
global using OpenTelemetry.Trace;
global using Microsoft.Extensions.Hosting;
global using System.Security.Claims;
global using Microsoft.Extensions.Configuration;
global using static OpenIddict.Abstractions.OpenIddictConstants;
