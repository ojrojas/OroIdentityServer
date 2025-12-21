// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.OroIdentityServer.Infraestructure.Data.EntityConfigurations;

public class RoleEntityConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id)
            .HasConversion(id => id.Value, value => new RoleId(value));

        builder.OwnsOne(x => x.Name, name =>
        {
            name.Property(n => n.Value)
            .HasColumnName("Name")
            .HasMaxLength(100)
            .IsRequired();
        });

        builder.Metadata
            .FindNavigation(nameof(Role.Claims))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.OwnsMany<RoleClaim>("_claims", claims =>
        {
            claims.ToTable("RoleClaims");
            claims.WithOwner().HasForeignKey("RoleId");

            claims.Property<Guid>("Id");
            claims.HasKey("Id");

            claims.OwnsOne(c => c.ClaimType, ct =>
            {
                ct.Property(rct => rct.Value)
                    .HasColumnName("ClaimType")
                    .HasMaxLength(100)
                    .IsRequired();
            });

            claims.OwnsOne(c => c.ClaimValue, cv =>
            {
                cv.Property(rcv => rcv.Value)
                    .HasColumnName("ClaimValue")
                    .HasMaxLength(200)
                    .IsRequired();
            });

            claims.Property(x => x.IsActive)
                .HasColumnName("IsActive");
        });
    }
}