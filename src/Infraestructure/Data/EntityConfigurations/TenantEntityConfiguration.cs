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

        builder.OwnsOne(it => it.Name, name =>
        {
            name.Property(n => n.Value)
                .HasColumnName("Name")
                .HasMaxLength(100)
                .IsRequired();
            name.HasIndex(n => n.Value)
                .IsUnique()
                .HasDatabaseName("IX_Tenants_Name");
        });

        builder.OwnsOne(t => t.Slug, slug => {
            slug.Property(s => s.Value)
            .HasColumnName("Slug")
            .HasMaxLength(100)
            .IsRequired();
        });

        // builder.HasIndex(it => it.Name.Value)
        //     .HasDatabaseName("IX_Tenants_Name")
        //     .IsUnique();

        builder.HasIndex(it => it.IsActive)
            .HasDatabaseName("IX_Tenants_IsActive");

        builder.HasQueryFilter(it => it.IsActive);

        // Ignore TenantUsers navigation for now - mapped separately when needed
        builder.Ignore(t => t.TenantUsers);
    }
}