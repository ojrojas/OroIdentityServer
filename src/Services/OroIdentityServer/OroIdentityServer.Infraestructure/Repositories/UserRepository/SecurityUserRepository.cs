// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.OroIdentityServer.Infraestructure.Repositories;

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
}