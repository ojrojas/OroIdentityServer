// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.OroIdentityServer.Infraestructure.Data.EntityConfigurations;

public class UserRolesEntityConfiguration : IEntityTypeConfiguration<UserRoles>
{
    public void Configure(EntityTypeBuilder<UserRoles> builder)
    {
        builder.HasKey(ur => new { ur.UserId, ur.RoleId });

        builder.HasOne<User>()
               .WithMany()
               .HasForeignKey(ur => ur.UserId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}