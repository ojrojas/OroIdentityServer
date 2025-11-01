// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.

using OroIdentityServer.OroIdentityServer.Infraestructure.Repositories.Extensions;
using OroIdentityServer.Services.OroIdentityServer.Api.Endpoints;
using OroIdentityServer.Services.OroIdentityServer.Application.Extensions;
using OroIdentityServer.Services.OroIdentityServer.Core.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

IConfiguration configuration = builder.Configuration;

Log.Logger = LoggerPrinter.CreateSerilogLogger("api", "OroIdentityServer", configuration);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.AddServicesWritersLogger(configuration);
builder.AddServiceDefaults();
builder.AddApplicationExtensions(configuration);
builder.AddApplicationExtensions(configuration);
builder.AddInfraestructureExtensions(configuration);
builder.AddCoreExtensions();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Endpoints
app.MapUsersEndpointsV1();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
