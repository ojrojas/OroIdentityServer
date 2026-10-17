// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Infraestructure.Data.Configurations;

public class MfaRecoveryCodeEntityConfiguration : IEntityTypeConfiguration<MfaRecoveryCode>
{
    public void Configure(EntityTypeBuilder<MfaRecoveryCode> builder)
    {
        builder.ToTable("MfaRecoveryCodes");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id)
            .HasConversion(id => id.Value, value => new MfaRecoveryCodeId(value))
            .HasColumnName("Id");

        builder.Property(c => c.MfaCredentialId)
            .HasConversion(id => id.Value, value => new MfaCredentialId(value))
            .HasColumnName("MfaCredentialId")
            .IsRequired();

        builder.Property(c => c.UserId)
            .HasConversion(id => id.Value, value => new UserId(value))
            .HasColumnName("UserId")
            .IsRequired();

        builder.Property(c => c.CodeHash)
            .HasColumnName("CodeHash")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(c => c.IsUsed).HasColumnName("IsUsed").IsRequired();
        builder.Property(c => c.UsedAtUtc).HasColumnName("UsedAtUtc");
        builder.Property(c => c.CreatedAtUtc).HasColumnName("CreatedAtUtc").IsRequired();

        builder.HasIndex(c => c.UserId).HasDatabaseName("IX_MfaRecoveryCodes_UserId");

        builder.HasOne<MfaCredential>()
            .WithMany()
            .HasForeignKey(c => c.MfaCredentialId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}