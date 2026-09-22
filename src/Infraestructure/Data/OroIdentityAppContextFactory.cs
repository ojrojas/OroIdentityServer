// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace OroIdentityServer.Infraestructure;

/// <summary>
/// Design-time factory used by <c>dotnet ef</c> to build the model without starting the host
/// (the runtime connection string comes from Aspire/environment configuration, which is not
/// available at design time). The connection string is a placeholder: migrations only need the
/// provider and the model, never an actual connection.
/// </summary>
public sealed class OroIdentityAppContextFactory : IDesignTimeDbContextFactory<OroIdentityAppContext>
{
    public OroIdentityAppContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<OroIdentityAppContext>()
            .UseNpgsql("Host=localhost;Database=oroidentity;Username=postgres;Password=postgres")
            .UseOpenIddict()
            .Options;

        return new OroIdentityAppContext(options);
    }
}
