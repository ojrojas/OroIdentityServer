// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Infraestructure.UnitTests;

internal static class TestDbContextFactory
{
    public static OroIdentityAppContext Create()
    {
        var options = new DbContextOptionsBuilder<OroIdentityAppContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new OroIdentityAppContext(options);
    }

    public static OroIdentityAppContext CreateSqlite()
    {
        var connection = new Microsoft.Data.Sqlite.SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<OroIdentityAppContext>()
            .UseSqlite(connection)
            .Options;

        var context = new OroIdentityAppContext(options);
        context.Database.EnsureCreated();
        return context;
    }
}
