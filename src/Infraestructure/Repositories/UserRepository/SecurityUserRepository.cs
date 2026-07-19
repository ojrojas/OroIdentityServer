// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using OroIdentityServer.Core.Modules.Users.ValueObjects;

namespace OroIdentityServer.Infraestructure.Repositories;

public class SecurityUserRepository(
    ILogger<SecurityUserRepository> logger,
    IRepository<SecurityUser> repository) : ISecurityUserRepository
{
    public async Task<SecurityUser> GetSecurityUserAsync(Guid securityId, CancellationToken cancellationToken)
    {
        logger.LogInformation("Get security user request by id");
        return await repository.GetByIdAsync(new SecurityUserId(securityId), cancellationToken)
               ?? throw new Exception("Security user not found");
    }

    public async Task UpdateSecurityUserAsync(SecurityUser securityUser, CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating security user {SecurityUserId}", securityUser.Id.Value);
        await repository.UpdateAsync(securityUser, cancellationToken);
    }
}