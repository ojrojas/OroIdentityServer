// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Infraestructure.Data.Configurations;

public class MfaCredentialEntityConfiguration : IEntityTypeConfiguration<MfaCredential>
{
    public void Configure(EntityTypeBuilder<MfaCredential> builder)
    {
        builder.ToTable("MfaCredentials");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id)
            .HasConversion(id => id.Value, value => new MfaCredentialId(value))
            .HasColumnName("Id");

        builder.Property(c => c.UserId)
            .HasConversion(id => id.Value, value => new UserId(value))
            .HasColumnName("UserId")
            .IsRequired();

        // One authenticator factor per user.
        builder.HasIndex(c => c.UserId).IsUnique().HasDatabaseName("IX_MfaCredentials_UserId");

        builder.Property(c => c.EncryptedSecret)
            .HasColumnName("EncryptedSecret")
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(c => c.IsEnabled).HasColumnName("IsEnabled").IsRequired();
        builder.Property(c => c.CreatedAtUtc).HasColumnName("CreatedAtUtc").IsRequired();
        builder.Property(c => c.ConfirmedAtUtc).HasColumnName("ConfirmedAtUtc");
        builder.Property(c => c.LastVerifiedAtUtc).HasColumnName("LastVerifiedAtUtc");
        builder.Property(c => c.LastUsedTimeStep).HasColumnName("LastUsedTimeStep").IsRequired();

        // Brute-force state. Indexed on LockedUntilUtc so the login path can cheaply ask "is this
        // credential serving a lockout right now" without loading every credential's history.
        builder.Property(c => c.FailedAttempts).HasColumnName("FailedAttempts").IsRequired();
        builder.Property(c => c.LastFailedAtUtc).HasColumnName("LastFailedAtUtc");
        builder.Property(c => c.LockedUntilUtc).HasColumnName("LockedUntilUtc");
        builder.HasIndex(c => c.LockedUntilUtc).HasDatabaseName("IX_MfaCredentials_LockedUntilUtc");

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}