// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.OroIdentityServer.Infraestructure.Repositories.Interfaces;

public interface IIdentificationTypeRepository
{
    Task AddIdentificationTypeAsync(IdentificationType identificationType, CancellationToken cancellationToken);
    Task UpdateIdentificationTypeAsync(IdentificationType identificationType, CancellationToken cancellationToken);
    Task DeleteIdentificationTypeAsync(Guid id, CancellationToken cancellationToken);
    Task<IdentificationType?> GetIdentificationTypeByIdAsync(Guid id);
    Task<IEnumerable<IdentificationType>> GetAllIdentificationTypesAsync();
}