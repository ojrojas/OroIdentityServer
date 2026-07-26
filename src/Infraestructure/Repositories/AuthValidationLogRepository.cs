// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Infraestructure.Repositories;

public class AuthValidationLogRepository(
    ILogger<AuthValidationLogRepository> logger,
    IRepository<AuthValidationLog> repository,
    OroIdentityAppContext context) : IAuthValidationLogRepository
{
    public async Task AddAsync(AuthValidationLog log, CancellationToken cancellationToken)
    {
        logger.LogInformation("Entering AddAsync");
        await repository.AddAsync(log, cancellationToken);
        logger.LogInformation("Exiting AddAsync");
    }

    public async Task<IReadOnlyList<AuthValidationLog>> GetSinceAsync(DateTime sinceUtc, CancellationToken cancellationToken)
    {
        logger.LogInformation("Entering GetSinceAsync with sinceUtc: {SinceUtc}", sinceUtc);
        var result = await repository.FindAsync(x => x.OccurredAtUtc >= sinceUtc, cancellationToken);
        logger.LogInformation("Exiting GetSinceAsync");
        return result.ToList();
    }

    public async Task<IReadOnlyList<AuthValidationLog>> GetRecentAsync(int take, CancellationToken cancellationToken)
    {
        logger.LogInformation("Entering GetRecentAsync with take: {Take}", take);
        var result = await context.Set<AuthValidationLog>()
            .AsNoTracking()
            .OrderByDescending(x => x.OccurredAtUtc)
            .Take(take)
            .ToListAsync(cancellationToken);
        logger.LogInformation("Exiting GetRecentAsync");
        return result;
    }
}
