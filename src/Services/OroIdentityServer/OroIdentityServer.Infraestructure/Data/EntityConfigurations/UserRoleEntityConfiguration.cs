// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Services.OroIdentityServer.Infraestructure;

public class UserRoleEntityConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("UserRoles"); // O el nombre que uses

        builder.HasKey(ur => new { ur.UserId, ur.RoleId }); // Clave compuesta

        builder.Property(ur => ur.UserId)
            .HasConversion(id => id!.Value, value => new UserId(value));

        builder.Property(ur => ur.RoleId)
            .HasConversion(id => id!.Value, value => new RoleId(value));

        // Configura las relaciones si es necesario
        builder.HasOne<User>() // Relación con User
            .WithMany(u => u.Roles)
            .HasForeignKey(ur => ur.UserId);

        builder.HasOne<Role>() // Relación con Role
            .WithMany() // Asume que Role no tiene colección inversa
            .HasForeignKey(ur => ur.RoleId);
    }
}