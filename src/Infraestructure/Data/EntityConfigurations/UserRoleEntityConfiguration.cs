// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Infraestructure.Data.Configurations;

public class UserRoleEntityConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("UserRoles");

        builder.HasKey(ur => new { ur.UserId, ur.RoleId });

        builder.Property(ur => ur.UserId)
            .HasConversion(id => id!.Value, value => new UserId(value));

        builder.Property(ur => ur.RoleId)
            .HasConversion(id => id!.Value, value => new RoleId(value));

        builder.HasOne<User>()
            .WithMany(u => u.Roles)
            .HasForeignKey(ur => ur.UserId);

        // The Role navigation is now a real C# property (UserRole.Role). Binding it here
        // makes EF populate it when callers do Include("Roles.Role") so sign-in and the
        // master-admin detector can read the catalogue role name in one round trip.
        builder.HasOne(ur => ur.Role)
            .WithMany()
            .HasForeignKey(ur => ur.RoleId);
    }
}