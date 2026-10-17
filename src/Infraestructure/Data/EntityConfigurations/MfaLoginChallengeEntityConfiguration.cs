// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Infraestructure.Data.Configurations;

public class MfaLoginChallengeEntityConfiguration : IEntityTypeConfiguration<MfaLoginChallenge>
{
    public void Configure(EntityTypeBuilder<MfaLoginChallenge> builder)
    {
        builder.ToTable("MfaLoginChallenges");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id)
            .HasConversion(id => id.Value, value => new MfaLoginChallengeId(value))
            .HasColumnName("Id");

        builder.Property(c => c.UserId)
            .HasConversion(id => id.Value, value => new UserId(value))
            .HasColumnName("UserId")
            .IsRequired();

        builder.Property(c => c.Token)
            .HasColumnName("Token")
            .HasMaxLength(64)
            .IsRequired();

        // The pending-MFA cookie carries this token; it is looked up on every code submission.
        builder.HasIndex(c => c.Token).IsUnique().HasDatabaseName("IX_MfaLoginChallenges_Token");
        builder.HasIndex(c => new { c.UserId, c.Status }).HasDatabaseName("IX_MfaLoginChallenges_UserId_Status");

        builder.Property(c => c.Purpose).HasColumnName("Purpose").IsRequired();
        builder.Property(c => c.Status).HasColumnName("Status").IsRequired();
        builder.Property(c => c.CreatedAtUtc).HasColumnName("CreatedAtUtc").IsRequired();
        builder.Property(c => c.ExpiresAtUtc).HasColumnName("ExpiresAtUtc").IsRequired();
        builder.Property(c => c.CompletedAtUtc).HasColumnName("CompletedAtUtc");
        builder.Property(c => c.Attempts).HasColumnName("Attempts").IsRequired();
        builder.Property(c => c.ReturnUrl).HasColumnName("ReturnUrl").HasMaxLength(400);
        builder.Property(c => c.IpAddress).HasColumnName("IpAddress").HasMaxLength(100);
        builder.Property(c => c.UserAgent).HasColumnName("UserAgent").HasMaxLength(1000);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}