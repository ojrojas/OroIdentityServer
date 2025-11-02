// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
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

builder.Services.Configure<RouteOptions>(options =>
{
    options.ConstraintMap["guid"] = typeof(GuidRouteConstraint);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}


#if DEBUG
var scope = app.Services.CreateScope();
var service = scope.ServiceProvider;

var context = service.GetRequiredService<OroIdentityAppContext>();
ArgumentNullException.ThrowIfNull(context);
await context.Database.EnsureDeletedAsync();
await context.Database.EnsureCreatedAsync();

#endif

app.UseHttpsRedirection();

// Endpoints
app.MapUsersQueriesEndpointsV1()
.WithTags("UserQueries");

app.MapUsersCommandsEndpointsV1()
.WithTags("UserCommands");

app.Run();
