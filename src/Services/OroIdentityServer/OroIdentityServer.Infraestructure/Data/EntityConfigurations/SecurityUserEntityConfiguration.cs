// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Services.OroIdentityServer.Infraestructure;

class SecurityUserEntityConfiguration : IEntityTypeConfiguration<SecurityUser>
{
    public void Configure(EntityTypeBuilder<SecurityUser> builder)
    {
        builder.ToTable("SecurityUsers");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new SecurityUserId(value))
            .HasColumnName("Id");
        builder.Property(x => x.SecurityStamp).IsConcurrencyToken();
    }
}