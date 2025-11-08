// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.OroIdentityServer.Infraestructure.Data.EntityConfigurations;

public class RoleClaimEntityConfiguration : IEntityTypeConfiguration<RoleClaim>
{
    public void Configure(EntityTypeBuilder<RoleClaim> builder)
    {
        builder.HasOne(rc => rc.Role)
               .WithMany()
               .HasForeignKey(rc => rc.RoleId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}