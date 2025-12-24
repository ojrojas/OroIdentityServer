// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.BuildingBlocks.Shared.Interfaces;

/// <summary>
/// OroIdentityServer Application DbContext interface
/// </summary>
public interface IOroAppDbContext
{
    /// <summary>
    /// Saves all changes made in this context to the database asynchronously.
    /// </summary>
    /// <param name="cancellationToken"> Cancellation token </param>
    /// <returns> Number of state entries written to the database </returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}