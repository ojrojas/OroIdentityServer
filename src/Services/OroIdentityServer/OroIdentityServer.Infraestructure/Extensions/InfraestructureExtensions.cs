// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.OroIdentityServer.Infraestructure.Repositories.Extensions;

public static class InfraestructureExtensions
{
    public static void AddInfraestructureExtensions(this IHostApplicationBuilder builder, IConfiguration configuration)
    {
        var connectionDatabase = configuration .GetConnectionString("");
        builder.Services.AddDbContext<OroIdentityAppContext>(options =>
        {
            options.UseNpgsql(connectionDatabase);
        });

        builder.Services.AddTransient(typeof(IRepository<>), typeof(Repository<>));
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<IRolesRepository, RolesRepository>();
        builder.Services.AddScoped<IIdentificationTypeRepository, IdentificationTypeRepository>();
    }
}