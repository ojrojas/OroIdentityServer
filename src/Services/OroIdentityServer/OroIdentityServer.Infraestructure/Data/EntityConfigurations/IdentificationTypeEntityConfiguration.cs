// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Services.OroIdentityServer.Infraestructure;

class IdentificationTypeEntityConfiguration : IEntityTypeConfiguration<IdentificationType>
{
    public void Configure(EntityTypeBuilder<IdentificationType> builder)
    {
        builder.ToTable("IdentificationTypes");
        builder.HasKey(x => x.Id);
        builder.OwnsOne(x => x.IdentificationTypeName, nv =>
        {
            nv.Property(p => p.Value)
            //.HasColumnName("IdentificationTypeName")
            .HasMaxLength(50)
            .IsRequired();
        });
    }
}