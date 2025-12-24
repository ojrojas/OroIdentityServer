// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Services.OroIdentityServer.Infraestructure;

public class OroIdentityAppContext(
    DbContextOptions<OroIdentityAppContext> options, IOptions<UserInfo> optionsUser) : AuditableDbContext(options, optionsUser)
{
    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<SecurityUser> SecurityUsers { get; set; }
    public DbSet<IdentificationType> IdentificationTypes { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasPostgresExtension("vector");
        builder.ApplyConfiguration(new UserEntityConfiguration());
        builder.ApplyConfiguration(new RoleEntityConfiguration());
        builder.ApplyConfiguration(new IdentificationTypeEntityConfiguration());
        builder.ApplyConfiguration(new UserRoleEntityConfiguration());

    }
}