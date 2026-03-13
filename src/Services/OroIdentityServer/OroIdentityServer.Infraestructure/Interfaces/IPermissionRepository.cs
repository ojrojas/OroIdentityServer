// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using OroIdentityServer.Services.OroIdentityServer.Core.Models;

namespace OroIdentityServer.OroIdentityServer.Infraestructure.Interfaces;

public interface IPermissionRepository
{
    Task AddPermissionAsync(Permission permission, CancellationToken cancellationToken);
    Task UpdatePermissionAsync(Permission permission, CancellationToken cancellationToken);
    Task DeletePermissionAsync(PermissionId id, CancellationToken cancellationToken);
    Task<Permission?> GetPermissionByIdAsync(PermissionId id, CancellationToken cancellationToken);
    Task<IEnumerable<Permission>> GetAllPermissionsAsync(CancellationToken cancellationToken);
}
