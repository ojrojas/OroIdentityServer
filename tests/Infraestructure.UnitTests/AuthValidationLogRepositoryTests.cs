// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using OroIdentityServer.Core.Modules.Diagnostics.Aggregates;
using OroIdentityServer.Core.Modules.Diagnostics.Enums;
using OroIdentityServer.Core.Modules.Diagnostics.Repositories;

namespace OroIdentityServer.Infraestructure.UnitTests;

public class AuthValidationLogRepositoryTests
{
    private static AuthValidationLogRepository CreateSut(OroIdentityAppContext context)
    {
        var repository = new Repository<AuthValidationLog>(NullLogger<Repository<AuthValidationLog>>.Instance, context);
        return new AuthValidationLogRepository(NullLogger<AuthValidationLogRepository>.Instance, repository);
    }

    [Fact]
    public async Task GetRecentAsync_ShouldReturnMostRecentFirst_LimitedByTake()
    {
        var context = TestDbContextFactory.Create();
        var sut = CreateSut(context);

        for (var i = 0; i < 5; i++)
        {
            var log = AuthValidationLog.Create(AuthValidationEventType.TokenIssued, true, null, "client", null, "127.0.0.1", null);
            context.AuthValidationLogs.Add(log);
            await context.SaveChangesAsync();
            await Task.Delay(5);
        }

        var result = await sut.GetRecentAsync(3, CancellationToken.None);

        Assert.Equal(3, result.Count);
        Assert.Equal(result.OrderByDescending(x => x.OccurredAtUtc), result);
    }
}
