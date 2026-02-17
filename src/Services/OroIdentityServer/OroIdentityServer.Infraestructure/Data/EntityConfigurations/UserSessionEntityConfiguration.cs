// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.

namespace OroIdentityServer.Services.OroIdentityServer.Infraestructure;

public class UserSessionEntityConfiguration : IEntityTypeConfiguration<UserSession>
{
    public void Configure(EntityTypeBuilder<UserSession> builder)
    {
        builder.ToTable("UserSessions");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id)
            .HasConversion(id => id.Value, value => new UserSessionId(value))
            .HasColumnName("Id");

        builder.Property(s => s.UserId)
            .HasConversion(id => id!.Value, value => new UserId(value))
            .HasColumnName("UserId");

        builder.HasIndex(s => s.SessionToken).IsUnique();

        builder.Property(s => s.Device).HasMaxLength(200);
        builder.Property(s => s.SessionToken).HasMaxLength(400);
        builder.Property(s => s.IpAddress).HasMaxLength(100);
        builder.Property(s => s.UserAgent).HasMaxLength(1000);
        builder.Property(s => s.Location).HasMaxLength(300);
    }
}
