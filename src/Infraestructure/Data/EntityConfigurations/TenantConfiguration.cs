// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Infraestructure.Data.Configurations;

public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("Tenants");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id)
            .HasConversion(id => id.Value, value => new TenantId(value))
            .HasColumnName("Id");

        builder.OwnsOne(t => t.Name, name =>
        {
            name.Property(n => n.Value).HasColumnName("Name").HasMaxLength(200).IsRequired();
        });

        // builder.OwnsOne(t => t.Slug, slug =>
        // {
        //     slug.Property(s => s).HasColumnName("Slug").HasMaxLength(50).IsRequired();
        //     slug.HasIndex(s => s.Value).IsUnique();
        // });

        builder.Property(t => t.IsActive).HasColumnName("IsActive").IsRequired();
        builder.Property(t => t.CreatedAtUtc).HasColumnName("CreatedAtUtc").IsRequired();

        builder.Metadata.FindNavigation(nameof(Tenant.TenantUsers))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(t => t.TenantUsers)
            .WithOne()
            .HasForeignKey(tu => tu.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(t => t.IsActive);
        builder.HasIndex(t => t.IsActive).HasDatabaseName("ix_tenants_is_active");
    }
}
