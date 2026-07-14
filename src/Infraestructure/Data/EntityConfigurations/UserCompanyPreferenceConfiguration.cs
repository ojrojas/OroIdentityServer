// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Infraestructure.Data.Configurations;

public class UserCompanyPreferenceConfiguration : IEntityTypeConfiguration<UserCompanyPreference>
{
    public void Configure(EntityTypeBuilder<UserCompanyPreference> builder)
    {
        builder.ToTable("UserCompanyPreferences");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id)
            .HasConversion(id => id.Value, value => UserCompanyPreferenceId.From(value))
            .HasColumnName("Id");

        builder.Property(p => p.TenantId).HasColumnName("TenantId").IsRequired();
        builder.Property(p => p.UserId).HasColumnName("UserId").HasMaxLength(200).IsRequired();
        builder.Property(p => p.CompanyId).HasColumnName("CompanyId").IsRequired();

        builder.Property(p => p.DefaultChartView)
            .HasConversion<string>().HasColumnName("DefaultChartView").HasMaxLength(20).IsRequired();

        builder.Property(p => p.DefaultReportPeriod)
            .HasConversion<string>().HasColumnName("DefaultReportPeriod").HasMaxLength(20).IsRequired();

        builder.HasIndex(p => new { p.UserId, p.TenantId, p.CompanyId }).IsUnique()
            .HasDatabaseName("ix_user_company_preferences_user_tenant_company");
    }
}
