// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Services.OroIdentityServer.Infraestructure;

public class SessionEntityConfiguration : IEntityTypeConfiguration<Session>
{
    public void Configure(EntityTypeBuilder<Session> builder)
    {
        builder.ToTable("Sessions");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id)
            .HasConversion(id => id.Value, value => new SessionId(value))
            .HasColumnName("Id");

        builder.Property(s => s.UserId)
            .HasConversion(id => id.Value, value => new UserId(value))
            .HasColumnName("UserId");

        builder.HasOne(s => s.User)
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(s => s.IpAddress).HasMaxLength(50);
        builder.Property(s => s.Country).HasMaxLength(100);
        builder.Property(s => s.StartedAtUtc).IsRequired();
        builder.Property(s => s.EndedAtUtc);
    }
}
