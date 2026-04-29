// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.OroIdentityServer.Infraestructure.Repositories.Extensions;

public static class InfraestructureExtensions
{
    public static void AddInfraestructureExtensions(this IHostApplicationBuilder builder, IConfiguration configuration)
    {
        var connectionDatabase = configuration.GetConnectionString("identitydb");
        builder.Services.AddDbContext<OroIdentityAppContext>(options =>
        {
            options.UseNpgsql(connectionDatabase);  
            options.UseOpenIddict();
        });

        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<IRoleRepository, RoleRepository>();
        builder.Services.AddScoped<IUserRolesRepository, UserRolesRepository>();
        builder.Services.AddScoped<IIdentificationTypeRepository, IdentificationTypeRepository>();
        builder.Services.AddScoped<ISecurityUserRepository, SecurityUserRepository>();
        builder.Services.AddScoped<IApplicationTenantRepository, ApplicationTenantRepository>();
        builder.Services.AddScoped<ITenantRepository, TenantRepository>();
        builder.Services.AddScoped<IPermissionRepository, PermissionRepository>();
        builder.Services.AddScoped<ISessionRepository, SessionRepository>();
        builder.Services.AddScoped<IUserSessionRepository, UserSessionRepository>();
    }
}