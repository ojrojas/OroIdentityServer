// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.OroIdentityServer.Infraestructure.Repositories;

public class IdentificationTypeRepository(
    ILogger<IdentificationTypeRepository> logger, IRepository<IdentificationType> repository)
: IIdentificationTypeRepository
{
    public async Task AddIdentificationTypeAsync(IdentificationType identificationType, CancellationToken cancellationToken)
    {
        logger.LogInformation("Entering AddIdentificationTypeAsync");
        await repository.AddAsync(identificationType, cancellationToken);
        logger.LogInformation("Exiting AddIdentificationTypeAsync");
    }

    public async Task DeleteIdentificationTypeAsync(Guid id, CancellationToken cancellationToken)
    {
        logger.LogInformation("Entering DeleteIdentificationTypeAsync with id: {Id}", id);
        var identificationType = await repository.GetByIdAsync(id);
        if (identificationType != null)
        {
            await repository.DeleteAsync(identificationType, cancellationToken);
        }
        logger.LogInformation("Exiting DeleteIdentificationTypeAsync");
    }

    public async Task<IEnumerable<IdentificationType>> GetAllIdentificationTypesAsync()
    {
        logger.LogInformation("Entering GetAllIdentificationTypesAsync");
        var result = await repository.GetAllAsync();
        logger.LogInformation("Exiting GetAllIdentificationTypesAsync");
        return result;
    }

    public async Task<IdentificationType?> GetIdentificationTypeByIdAsync(Guid id)
    {
        logger.LogInformation("Entering GetIdentificationTypeByIdAsync with id: {Id}", id);
        var result = await repository.GetByIdAsync(id);
        logger.LogInformation("Exiting GetIdentificationTypeByIdAsync");
        return result;
    }

    public async Task UpdateIdentificationTypeAsync(IdentificationType identificationType, CancellationToken cancellationToken)
    {
        logger.LogInformation("Entering UpdateIdentificationTypeAsync");
        await repository.UpdateAsync(identificationType, cancellationToken);
        logger.LogInformation("Exiting UpdateIdentificationTypeAsync");
    }
}
