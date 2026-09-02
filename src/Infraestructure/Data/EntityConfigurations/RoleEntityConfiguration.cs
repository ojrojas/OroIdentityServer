// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Infraestructure.Data.Configurations;

public class RoleEntityConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id)
            .HasConversion(id => id.Value, value => new RoleId(value))
            .HasColumnName("Id");

        builder.Property(r => r.IsActive)
            .HasColumnName("IsActive")
            .IsRequired();

        builder.Property(r => r.CreatedAtUtc)
            .HasColumnName("CreatedAtUtc")
            .IsRequired();

        builder.Property(r => r.Level)
            .HasColumnName("Level")
            .IsRequired()
            .HasDefaultValue(10);

        builder.Property(r => r.ParentRoleId)
            .HasConversion(
                id => id != null ? id.Value : (Guid?)null,
                value => value != null ? new RoleId(value.Value) : null)
            .HasColumnName("ParentRoleId")
            .IsRequired(false);

        builder.HasOne(r => r.ParentRole)
            .WithMany()
            .HasForeignKey(r => r.ParentRoleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(r => r.Name)
            .HasConversion(
                name => name.Value,
                value => new RoleName(value))
            .HasColumnName("Name")
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(r => r.Name)
            .IsUnique()
            .HasDatabaseName("IX_Roles_Name");

        builder.Metadata
         .FindNavigation(nameof(Role.RolePermissions))!
         .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(r => r.RolePermissions)
        .WithOne()
        .HasForeignKey(nameof(RolePermission.RoleId))
        .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(r => r.IsActive)
            .HasDatabaseName("IX_Roles_IsActive");

        builder.HasQueryFilter(r => r.IsActive);
    }
}
