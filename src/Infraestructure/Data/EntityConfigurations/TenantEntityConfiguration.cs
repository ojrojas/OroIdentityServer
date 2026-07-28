// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Infraestructure.Data.Configurations;

public class TenantEntityConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("Tenants");

        builder.HasKey(it => it.Id);
        builder.Property(it => it.Id)
            .HasConversion(id => id.Value, value => new TenantId(value))
            .HasColumnName("Id");

        builder.Property(it => it.IsActive)
            .HasColumnName("IsActive")
            .IsRequired();

        builder.Property(it => it.Name)
            .HasConversion(
                name => name.Value,
                value => new TenantName(value))
            .HasColumnName("Name")
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(it => it.Name)
            .IsUnique()
            .HasDatabaseName("IX_Tenants_Name");

        builder.Property(t => t.Slug)
            .HasConversion(
                slug => slug.Value,
                value => new TenantSlug(value))
            .HasColumnName("Slug")
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(it => it.IsActive)
            .HasDatabaseName("IX_Tenants_IsActive");

        builder.HasQueryFilter(it => it.IsActive);

        builder.Metadata.FindNavigation(nameof(Tenant.TenantUsers))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(t => t.TenantUsers)
            .WithOne()
            .HasForeignKey(tu => tu.TenantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}