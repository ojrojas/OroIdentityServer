// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.OroIdentityServer.Infraestructure.Repositories.Interfaces;

public interface ISecurityUserRepository
{
    Task<SecurityUser> GetSecurityUserAsync(Guid securityId, CancellationToken cancellationToken);
}
