// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Infraestructure.Data.Configurations;

public class TenantUserConfiguration : IEntityTypeConfiguration<TenantUser>
{
    public void Configure(EntityTypeBuilder<TenantUser> builder)
    {
        builder.ToTable("TenantUsers");
        builder.HasKey(tu => tu.TenantUserId);
        builder.Property(tu => tu.TenantUserId)
            .HasConversion(id => id.Value, value => new TenantUserId(value))
            .HasColumnName("Id");

        builder.Property(tu => tu.TenantId)
            .HasConversion(id => id.Value, value => new TenantId(value))
            .HasColumnName("TenantId")
            .IsRequired();

        builder.Property(tu => tu.UserId)
            .HasConversion(id => id.Value, value => new UserId(value))
            .HasColumnName("UserId")
            .IsRequired();

        builder.Property(tu => tu.UserRoles).HasColumnName("Role").HasMaxLength(50).IsRequired();
        builder.Property(tu => tu.IsActive).HasColumnName("IsActive").IsRequired();
        builder.Property(tu => tu.JoinedAtUtc).HasColumnName("JoinedAtUtc").IsRequired();

        builder.HasIndex(tu => new { tu.TenantId, tu.UserId })
            .IsUnique()
            .HasDatabaseName("ix_tenant_users_tenant_id_user_id");
    }
}
