// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.BuildingBlocks.Shared.Data;

public abstract class AuditableDbContext(DbContextOptions options, IOptions<UserInfo> optionsUser) : DbContext(options)
{
    private const int First = 0;
    private readonly UserInfo _userInfo = optionsUser.Value;

    // Audit tables
    public DbSet<AuditEntry> AuditEntries { get; set; }
    public DbSet<AuditEntryProperty> AuditEntryProperties { get; set; }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var auditEntries = OnBeforeSaveChanges();
        var result = await base.SaveChangesAsync(cancellationToken);
        await OnAfterSaveChanges(auditEntries);

        return result;
    }

    private List<AuditEntry> OnBeforeSaveChanges()
    {
        ChangeTracker.DetectChanges();
        var auditEntries = new List<AuditEntry>();

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is AuditEntry || entry.Entity is AuditEntryProperty)
                continue;

            if (entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                continue;

            var auditEntry = new AuditEntry
            {
                EntityName = entry.Metadata.GetTableName() ?? entry.Metadata.Name,
                Timestamp = DateTimeOffset.UtcNow.ToUniversalTime(),
                UserId = _userInfo.Id,
                UserName = _userInfo.UserName,
                State = entry.State
            };

            switch (entry.State)
            {
                case EntityState.Added:
                    auditEntry.Action = "Added";
                    break;
                case EntityState.Modified:
                    auditEntry.Action = "Modified";
                    foreach (var property in entry.Properties)
                    {
                        if (property.IsModified && !property.Metadata.IsForeignKey() && !property.Metadata.IsShadowProperty())
                        {
                            auditEntry.TemporaryProperties.Add(new PropertyChange
                            {
                                PropertyName = property.Metadata.Name,
                                OldValue = property.OriginalValue?.ToString(),
                                NewValue = property.CurrentValue?.ToString()
                            });
                        }
                    }
                    break;
                case EntityState.Deleted:
                    auditEntry.Action = "Deleted";
                    foreach (var property in entry.Properties)
                    {
                        auditEntry.TemporaryProperties.Add(new PropertyChange
                        {
                            PropertyName = property.Metadata.Name,
                            OldValue = property.OriginalValue?.ToString(),
                            NewValue = null
                        });
                    }
                    break;
            }
            auditEntries.Add(auditEntry);
        }
        return auditEntries;
    }

    private async Task OnAfterSaveChanges(List<AuditEntry> auditEntries)
    {
        if (auditEntries == null || auditEntries.Count == 0) return;

        foreach (var auditEntry in auditEntries)
        {
            var entry = ChangeTracker.Entries().FirstOrDefault(e =>
                e.Metadata.GetTableName() == auditEntry.EntityName &&
                (e.State == auditEntry.State ||
                 (auditEntry.State == EntityState.Added && e.State == EntityState.Unchanged)));

            if (entry != null)
            {
                auditEntry.EntityId = entry.Property(entry.Metadata.FindPrimaryKey()!.Properties[First].Name).CurrentValue?.ToString() ?? "N/A";

                if (auditEntry.TemporaryProperties.Count != 0)
                {
                    auditEntry.ChangesJson = JsonSerializer.Serialize(auditEntry.TemporaryProperties);
                }

                foreach (var propChange in auditEntry.TemporaryProperties)
                {
                    auditEntry.Properties.Add(new AuditEntryProperty
                    {
                        PropertyName = propChange.PropertyName,
                        OldValue = propChange.OldValue,
                        NewValue = propChange.NewValue
                    });
                }
            }
            AuditEntries.Add(auditEntry);
        }

        await base.SaveChangesAsync();
    }

    /// <summary>
    /// On model creating database, and specific change model
    /// </summary>
    /// <param name="modelBuilder">Model builder application</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(assembly: Assembly.GetExecutingAssembly());
    }
}