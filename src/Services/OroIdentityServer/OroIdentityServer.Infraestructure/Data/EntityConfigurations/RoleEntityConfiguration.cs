// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.OroIdentityServer.Infraestructure.Data.EntityConfigurations;

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
        });

        builder.HasIndex(r => r.IsActive)
            .HasDatabaseName("IX_Roles_IsActive");

        builder.HasQueryFilter(r => r.IsActive);
    }
}
