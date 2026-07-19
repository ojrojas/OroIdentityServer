// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using Microsoft.EntityFrameworkCore;
using OroIdentityServer.Infraestructure;

namespace OroIdentityServer.Application.Modules.Tenants.Services;

public class TenantSchemaProvisioner(
    OroIdentityAppContext dbContext,
    ILogger<TenantSchemaProvisioner> logger) : ITenantSchemaProvisioner
{
    private static readonly System.Text.RegularExpressions.Regex SlugPattern =
        new(@"^[a-z0-9_]+$", System.Text.RegularExpressions.RegexOptions.Compiled);

    public async Task ProvisionSchemaAsync(string slug, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(slug) || !SlugPattern.IsMatch(slug))
        {
            throw new ArgumentException($"Invalid tenant slug '{slug}'. Slug must contain only lowercase letters, digits and underscores.", nameof(slug));
        }

        logger.LogInformation("Creating PostgreSQL schema for tenant slug: {Slug}", slug);

        // Schema names cannot be parameterized; slug is validated above against SlugPattern.
#pragma warning disable EF1002
        await dbContext.Database.ExecuteSqlRawAsync(
            $"CREATE SCHEMA IF NOT EXISTS \"{slug}\"",
            cancellationToken);
#pragma warning restore EF1002

        logger.LogInformation("PostgreSQL schema created for tenant slug: {Slug}", slug);
    }
}
