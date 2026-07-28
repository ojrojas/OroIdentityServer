// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Infraestructure;

public class OroIdentityAppContext(
    DbContextOptions<OroIdentityAppContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<SecurityUser> SecurityUsers { get; set; }
    public DbSet<IdentificationType> IdentificationTypes { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<TenantUser> TenantUsers { get; set; }
    public DbSet<TenantPreferenceConfig> TenantPreferenceConfigs { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }
    public DbSet<Session> Sessions { get; set; }
    public DbSet<UserSession> UserSessions { get; set; }
    public DbSet<AuthValidationLog> AuthValidationLogs { get; set; }
    public DbSet<UserPreference> UserPreferences { get; set; }
    public DbSet<UserCompanyPreference> UserCompanyPreferences { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {

        builder.ApplyConfiguration(new ApplicationTenantEntityConfiguration());
        builder.ApplyConfiguration(new AuthValidationLogEntityConfiguration());
        builder.ApplyConfiguration(new IdentificationTypeEntityConfiguration());
        builder.ApplyConfiguration(new PermissionEntityConfiguration());
        builder.ApplyConfiguration(new RoleEntityConfiguration());
        builder.ApplyConfiguration(new RolePermissionEntityConfiguration());
        builder.ApplyConfiguration(new SecurityUserEntityConfiguration());
        builder.ApplyConfiguration(new SessionEntityConfiguration());
        builder.ApplyConfiguration(new TenantEntityConfiguration());
        builder.ApplyConfiguration(new TenantPreferenceConfigConfiguration());
        builder.ApplyConfiguration(new TenantUserConfiguration());
        builder.ApplyConfiguration(new UserCompanyPreferenceConfiguration());
        builder.ApplyConfiguration(new UserEntityConfiguration());
        builder.ApplyConfiguration(new UserPreferenceConfiguration());
        builder.ApplyConfiguration(new UserRoleEntityConfiguration());
        builder.ApplyConfiguration(new UserSessionEntityConfiguration());
    }
}