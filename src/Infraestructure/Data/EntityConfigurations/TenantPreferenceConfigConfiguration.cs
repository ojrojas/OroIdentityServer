// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Infraestructure.Data.Configurations;

public class TenantPreferenceConfigConfiguration : IEntityTypeConfiguration<TenantPreferenceConfig>
{
    public void Configure(EntityTypeBuilder<TenantPreferenceConfig> builder)
    {
        builder.ToTable("TenantPreferenceConfigs");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id)
            .HasConversion(id => id.Value, value => TenantPreferenceConfigId.From(value))
            .HasColumnName("Id");

        builder.Property(c => c.TenantId).HasColumnName("TenantId").IsRequired();

        builder.Property(c => c.DefaultLanguage)
            .HasConversion<string>().HasColumnName("DefaultLanguage").HasMaxLength(10).IsRequired();

        builder.Property(c => c.DefaultTimezone)
            .HasConversion(tz => tz.Value, value => Timezone.From(value))
            .HasColumnName("DefaultTimezone").HasMaxLength(100).IsRequired();

        builder.Property(c => c.DefaultDateFormat)
            .HasConversion<string>().HasColumnName("DefaultDateFormat").HasMaxLength(20).IsRequired();

        builder.Property(c => c.DefaultNumberFormat)
            .HasConversion<string>().HasColumnName("DefaultNumberFormat").HasMaxLength(20).IsRequired();

        builder.Property(c => c.DefaultTheme)
            .HasConversion<string>().HasColumnName("DefaultTheme").HasMaxLength(20).IsRequired();

        builder.Property(c => c.ForceLanguage).HasColumnName("ForceLanguage").IsRequired();
        builder.Property(c => c.ForceTheme).HasColumnName("ForceTheme").IsRequired();

        builder.HasIndex(c => c.TenantId).IsUnique().HasDatabaseName("ix_tenant_preference_configs_tenant_id");
    }
}
