// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
using OroIdentityServer.Core.Modules.Hierarchy.Entities;
using OroIdentityServer.Core.Modules.Hierarchy.ValueObjects;

namespace OroIdentityServer.Infraestructure.Data.Configurations;

public class RelationshipAuditLogEntityConfiguration : IEntityTypeConfiguration<RelationshipAuditLog>
{
    public void Configure(EntityTypeBuilder<RelationshipAuditLog> builder)
    {
        builder.ToTable("RelationshipAuditLogs");

        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id)
            .HasConversion(id => id.Value, value => new RelationshipAuditLogId(value))
            .HasColumnName("Id");

        builder.Property(l => l.RelationshipId)
            .HasConversion(id => id.Value, value => new UserReportingRelationshipId(value))
            .HasColumnName("RelationshipId")
            .IsRequired();

        builder.Property(l => l.TenantId)
            .HasConversion(id => id.Value, value => new TenantId(value))
            .HasColumnName("TenantId")
            .IsRequired();

        builder.Property(l => l.UserId)
            .HasConversion(id => id.Value, value => new UserId(value))
            .HasColumnName("UserId")
            .IsRequired();

        builder.Property(l => l.ReportsToUserId)
            .HasConversion(id => id.Value, value => new UserId(value))
            .HasColumnName("ReportsToUserId")
            .IsRequired();

        builder.Property(l => l.RelationshipType)
            .HasConversion<string>()
            .HasColumnName("RelationshipType")
            .IsRequired();

        builder.Property(l => l.Action)
            .HasConversion<string>()
            .HasColumnName("Action")
            .IsRequired();

        builder.Property(l => l.PerformedByUserId)
            .HasConversion(
                id => id != null ? id.Value : (Guid?)null,
                value => value != null ? new UserId(value.Value) : null)
            .HasColumnName("PerformedByUserId")
            .IsRequired(false);

        builder.Property(l => l.PerformedAtUtc)
            .HasColumnName("PerformedAtUtc")
            .IsRequired();

        builder.Property(l => l.Details)
            .HasColumnName("Details")
            .HasMaxLength(1000)
            .IsRequired(false);

        builder.Property(l => l.Reason)
            .HasColumnName("Reason")
            .HasMaxLength(1000)
            .IsRequired(false);

        builder.HasIndex(l => l.RelationshipId)
            .HasDatabaseName("IX_RelationshipAuditLogs_RelationshipId");

        builder.HasIndex(l => new { l.TenantId, l.UserId })
            .HasDatabaseName("IX_RelationshipAuditLogs_Tenant_User");

        builder.HasIndex(l => l.PerformedAtUtc)
            .HasDatabaseName("IX_RelationshipAuditLogs_PerformedAtUtc");
    }
}
