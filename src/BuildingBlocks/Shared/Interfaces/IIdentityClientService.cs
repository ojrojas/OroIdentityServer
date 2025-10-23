// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.BuildingBlocks.Shared.Interfaces;

public interface IIdentityClientService
{
    /// <summary>
    /// Get basic info by ID.
    /// </summary>
    /// <param name="userId">User identifier.</param>
    /// <param name="cancellationToken">Token operation cancellation .</param>
    /// <returns>UserInfo: user related information</returns>
    Task<UserInfo?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get Roles ids by user id request
    /// </summary>
    /// <param name="userId">UserId</param>
    /// <param name="cancellationToken">Token operation cancellation</param>
    /// <returns>RoleId collection</returns>
    Task<IEnumerable<Guid>> GetUserRoleIdsAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get role info by role id
    /// </summary>
    /// <param name="roleId">RoleId</param>
    /// <param name="cancellationToken">Token operation cancellation</param>
    /// <returns>Get Roleinfo</returns>
    Task<RoleInfo?> GetRoleByIdAsync(Guid roleId, CancellationToken cancellationToken = default);
}