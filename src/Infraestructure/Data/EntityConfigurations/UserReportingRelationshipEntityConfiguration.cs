// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
using OroIdentityServer.Core.Modules.Hierarchy.Entities;
using OroIdentityServer.Core.Modules.Hierarchy.Enums;
using OroIdentityServer.Core.Modules.Hierarchy.ValueObjects;

namespace OroIdentityServer.Infraestructure.Data.Configurations;

public class UserReportingRelationshipEntityConfiguration : IEntityTypeConfiguration<UserReportingRelationship>
{
    public void Configure(EntityTypeBuilder<UserReportingRelationship> builder)
    {
        builder.ToTable("UserReportingRelationships");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id)
            .HasConversion(id => id.Value, value => new UserReportingRelationshipId(value))
            .HasColumnName("Id");

        builder.Property(r => r.TenantId)
            .HasConversion(id => id.Value, value => new TenantId(value))
            .HasColumnName("TenantId")
            .IsRequired();

        builder.Property(r => r.UserId)
            .HasConversion(id => id.Value, value => new UserId(value))
            .HasColumnName("UserId")
            .IsRequired();

        builder.Property(r => r.ReportsToUserId)
            .HasConversion(id => id.Value, value => new UserId(value))
            .HasColumnName("ReportsToUserId")
            .IsRequired();

        builder.Property(r => r.RelationshipType)
            .HasConversion<string>()
            .HasColumnName("RelationshipType")
            .IsRequired();

        builder.Property(r => r.Priority)
            .HasColumnName("Priority")
            .IsRequired();

        builder.Property(r => r.IsActive)
            .HasColumnName("IsActive")
            .IsRequired();

        builder.Property(r => r.CreatedAtUtc)
            .HasColumnName("CreatedAtUtc")
            .IsRequired();

        builder.Property(r => r.UpdatedAtUtc)
            .HasColumnName("UpdatedAtUtc")
            .IsRequired(false);

        builder.Property(r => r.CreatedByUserId)
            .HasConversion(
                id => id != null ? id.Value : (Guid?)null,
                value => value != null ? new UserId(value.Value) : null)
            .HasColumnName("CreatedByUserId")
            .IsRequired(false);

        builder.HasIndex(r => new { r.TenantId, r.UserId, r.ReportsToUserId, r.RelationshipType })
            .IsUnique()
            .HasDatabaseName("IX_UserReportingRelationships_Unique");

        builder.HasIndex(r => new { r.TenantId, r.ReportsToUserId })
            .HasDatabaseName("IX_UserReportingRelationships_Tenant_ReportsTo");

        builder.HasIndex(r => new { r.TenantId, r.UserId })
            .HasDatabaseName("IX_UserReportingRelationships_Tenant_User");

        builder.HasIndex(r => r.IsActive)
            .HasDatabaseName("IX_UserReportingRelationships_IsActive");

        builder.HasQueryFilter(r => r.IsActive);

        builder.HasOne(r => r.Tenant)
            .WithMany()
            .HasForeignKey(r => r.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.ReportsToUser)
            .WithMany()
            .HasForeignKey(r => r.ReportsToUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
