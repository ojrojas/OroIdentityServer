// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Services.OroIdentityServer.Infraestructure;

public class IdentificationTypeEntityConfiguration : IEntityTypeConfiguration<IdentificationType>
{
    public void Configure(EntityTypeBuilder<IdentificationType> builder)
    {
        builder.ToTable("IdentificationTypes");

        builder.HasKey(it => it.Id);
        builder.Property(it => it.Id)
            .HasConversion(id => id.Value, value => new IdentificationTypeId(value))
            .HasColumnName("Id");

        builder.Property(it => it.IsActive)
            .HasColumnName("IsActive")
            .IsRequired();

        builder.OwnsOne(it => it.Name, name =>
        {
            name.Property(n => n.Value)
                .HasColumnName("Name")
                .HasMaxLength(100)
                .IsRequired();
            name.HasIndex(n => n.Value)
                .IsUnique()
                .HasDatabaseName("IX_IdentificationTypes_Name");
        });

        // builder.HasIndex(it => it.Name.Value)
        //     .HasDatabaseName("IX_IdentificationTypes_Name")
        //     .IsUnique();

        builder.HasIndex(it => it.IsActive)
            .HasDatabaseName("IX_IdentificationTypes_IsActive");

        builder.HasQueryFilter(it => it.IsActive);
    }
}