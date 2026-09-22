// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Infraestructure.Data.Configurations;

public class UserPermissionEntityConfiguration : IEntityTypeConfiguration<UserPermission>
{
    public void Configure(EntityTypeBuilder<UserPermission> builder)
    {
        builder.ToTable("UserPermissions");

        builder.HasKey(up => new { up.UserId, up.PermissionId });

        builder.Property(up => up.UserId)
            .HasConversion(id => id!.Value, value => new UserId(value));

        builder.Property(up => up.PermissionId)
            .HasConversion(id => id!.Value, value => new PermissionId(value));

        builder.HasOne<User>()
            .WithMany(u => u.Permissions)
            .HasForeignKey(up => up.UserId);
    }
}
