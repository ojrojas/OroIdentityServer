// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Infraestructure;

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

        builder.OwnsOne(r => r.Name, name =>
        {
            name.Property(n => n.Value)
                .HasColumnName("Name")
                .HasMaxLength(100)
                .IsRequired();
        });

        builder.Metadata
            .FindNavigation(nameof(Role.Claims))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.OwnsMany(r => r.Claims, claims =>
        {
            claims.ToTable("RoleClaims");
            claims.WithOwner().HasForeignKey("RoleId");

            // Explicit shadow FK column to match principal key (Role.Id -> RoleId conversion)
            claims.Property<Guid>("RoleId").HasColumnName("RoleId").IsRequired();

            claims.Property(rc => rc.Id)
                .HasColumnName("Id")
                .ValueGeneratedOnAdd();

            claims.HasKey(rc => rc.Id);

            claims.Property(rc => rc.IsActive)
                .HasColumnName("IsActive")
                .IsRequired();

            claims.OwnsOne(rc => rc.ClaimType, ct =>
            {
                ct.Property(rct => rct.Value)
                    .HasColumnName("ClaimType")
                    .HasMaxLength(100)
                    .IsRequired();
            });

            claims.OwnsOne(rc => rc.ClaimValue, cv =>
            {
                cv.Property(rcv => rcv.Value)
                    .HasColumnName("ClaimValue")
                    .HasMaxLength(500) 
                    .IsRequired();
            });

            // Unique index to prevent duplicate role claims at database level
            // NOTE: index creation for owned type columns causes model-building conflicts at design-time
            // (EF sometimes tries to add shadow properties with the same name as navigation properties).
            // To avoid design-time DbContext errors when creating migrations, create the unique index
            // using raw SQL inside the migration Up() instead of configuring it here.
            // claims.HasIndex("RoleId", "ClaimType", "ClaimValue")
            //     .IsUnique()
            //     .HasDatabaseName("IX_RoleClaims_RoleId_ClaimType_ClaimValue_Unique");
        });

        builder.HasIndex(r => r.IsActive)
            .HasDatabaseName("IX_Roles_IsActive");

        builder.HasQueryFilter(r => r.IsActive);
    }
}
