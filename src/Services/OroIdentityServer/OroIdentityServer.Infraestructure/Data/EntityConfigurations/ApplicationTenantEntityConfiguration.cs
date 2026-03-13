// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OroIdentityServer.Services.OroIdentityServer.Core.Models;
using OroIdentityServer.Services.OroIdentityServer.Infraestructure.Data;

namespace OroIdentityServer.Services.OroIdentityServer.Infraestructure;

public class ApplicationTenantEntityConfiguration : IEntityTypeConfiguration<ApplicationTenant>
{
    public void Configure(EntityTypeBuilder<ApplicationTenant> builder)
    {
        builder.ToTable("ApplicationTenants");

        builder.HasKey(at => at.ClientId);
        builder.Property(at => at.ClientId)
            .HasColumnName("ClientId")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(at => at.TenantId)
            .HasConversion(id => id.Value, value => new TenantId(value))
            .HasColumnName("TenantId")
            .IsRequired();

        builder.HasIndex(at => at.TenantId).HasDatabaseName("IX_ApplicationTenants_TenantId");
    }
}
