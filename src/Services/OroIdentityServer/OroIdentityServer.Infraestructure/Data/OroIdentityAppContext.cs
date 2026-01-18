// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
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
    public DbSet<UserRole> UserRoles { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // Owned types are configured in their respective entity configurations
        // builder.Owned<IdentificationTypeName>(b =>
        // {
        //     b.Property(n => n.Value).HasColumnName("Name").HasMaxLength(100).IsRequired();
        // });
        // builder.Owned<RoleName>(b =>
        // {
        //     b.Property(n => n.Value).HasColumnName("Name").HasMaxLength(100).IsRequired();
        // });
        // builder.Owned<TenantName>(b =>
        // {
        //     b.Property(n => n.Value).HasColumnName("Name").HasMaxLength(100).IsRequired();
        // });
        // builder.Owned<RoleClaimType>(b =>
        // {
        //     b.Property(n => n.Value).HasColumnName("ClaimType").HasMaxLength(100).IsRequired();
        // });
        // builder.Owned<RoleClaimValue>(b =>
        // {
        //     b.Property(n => n.Value).HasColumnName("ClaimValue").HasMaxLength(500).IsRequired();
        // });

        builder.ApplyConfiguration(new UserEntityConfiguration());
        builder.ApplyConfiguration(new RoleEntityConfiguration());
        builder.ApplyConfiguration(new IdentificationTypeEntityConfiguration());
        builder.ApplyConfiguration(new UserRoleEntityConfiguration());
        builder.ApplyConfiguration(new SecurityUserEntityConfiguration());

    }
}