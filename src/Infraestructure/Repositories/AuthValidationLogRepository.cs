// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Infraestructure.Repositories;

public class AuthValidationLogRepository(
    ILogger<AuthValidationLogRepository> logger,
    IRepository<AuthValidationLog> repository) : IAuthValidationLogRepository
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
}
