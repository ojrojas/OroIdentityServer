// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Services.OroIdentityServer.Infraestructure;

public class UserEntityConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id)
            .HasConversion(id => id.Value, value => new UserId(value))
            .HasColumnName("Id");

        builder.HasIndex(u => u.Email).IsUnique();
        builder.HasIndex(u => u.UserName).IsUnique();
        builder.HasIndex(u => u.Identification).IsUnique();

        builder.HasOne(u => u.IdentificationType)
            .WithMany() 
            .HasForeignKey(u => u.IdentificationTypeId)
            .OnDelete(DeleteBehavior.Restrict); 

        builder.HasOne(u => u.SecurityUser)
            .WithOne()
            .HasForeignKey<User>(u => u.SecurityUserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}