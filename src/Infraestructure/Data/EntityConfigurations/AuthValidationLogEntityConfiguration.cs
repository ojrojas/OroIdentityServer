// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Infraestructure.Data.Configurations;

public class AuthValidationLogEntityConfiguration : IEntityTypeConfiguration<AuthValidationLog>
{
    public void Configure(EntityTypeBuilder<AuthValidationLog> builder)
    {
        builder.ToTable("AuthValidationLogs");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new AuthValidationLogId(value))
            .HasColumnName("Id");

        builder.Property(x => x.EventType)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(x => x.Succeeded).IsRequired();
        builder.Property(x => x.UserId);
        builder.Property(x => x.ClientId).HasMaxLength(200);
        builder.Property(x => x.Scopes);
        builder.Property(x => x.IpAddress).HasMaxLength(50);
        builder.Property(x => x.FailureReason).HasMaxLength(500);
        builder.Property(x => x.OccurredAtUtc).IsRequired();

        builder.HasIndex(x => x.OccurredAtUtc)
            .HasDatabaseName("IX_AuthValidationLogs_OccurredAtUtc");
    }
}
