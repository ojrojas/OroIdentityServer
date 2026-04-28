// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Infraestructure.Data.Configurations;

public class UserPreferenceConfiguration : IEntityTypeConfiguration<UserPreference>
{
    public void Configure(EntityTypeBuilder<UserPreference> builder)
    {
        builder.ToTable("UserPreferences");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id)
            .HasConversion(id => id.Value, value => UserPreferenceId.From(value))
            .HasColumnName("Id");

        builder.Property(p => p.TenantId).HasColumnName("TenantId").IsRequired();
        builder.Property(p => p.UserId).HasColumnName("UserId").HasMaxLength(200).IsRequired();

        builder.Property(p => p.Language)
            .HasConversion<string>().HasColumnName("Language").HasMaxLength(10).IsRequired();

        builder.Property(p => p.Timezone)
            .HasConversion(tz => tz.Value, value => Timezone.From(value))
            .HasColumnName("Timezone").HasMaxLength(100).IsRequired();

        builder.Property(p => p.DateFormat)
            .HasConversion<string>().HasColumnName("DateFormat").HasMaxLength(20).IsRequired();

        builder.Property(p => p.NumberFormat)
            .HasConversion<string>().HasColumnName("NumberFormat").HasMaxLength(20).IsRequired();

        builder.Property(p => p.Theme)
            .HasConversion<string>().HasColumnName("Theme").HasMaxLength(20).IsRequired();

        builder.Property(p => p.DefaultCompanyId).HasColumnName("DefaultCompanyId");

        builder.Property(p => p.InboxSortField)
            .HasConversion<string>().HasColumnName("InboxSortField").HasMaxLength(20).IsRequired();

        builder.Property(p => p.InboxSortDirection)
            .HasConversion<string>().HasColumnName("InboxSortDirection").HasMaxLength(20).IsRequired();

        builder.Property(p => p.DashboardLayout).HasColumnName("DashboardLayout");
        builder.Property(p => p.SidebarCollapsed).HasColumnName("SidebarCollapsed").IsRequired();
        builder.Property(p => p.CreatedAtUtc).HasColumnName("CreatedAtUtc").IsRequired();
        builder.Property(p => p.UpdatedAtUtc).HasColumnName("UpdatedAtUtc").IsRequired();

        builder.HasIndex(p => new { p.UserId, p.TenantId }).IsUnique().HasDatabaseName("ix_user_preferences_user_tenant");
    }
}
