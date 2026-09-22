// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Application.Modules.Permissions.Queries;

public sealed class GetPermissionNamesByUserIdQueryHandler(
    ILogger<GetPermissionNamesByUserIdQueryHandler> logger,
    IPermissionRepository permissionRepository)
    : IQueryHandler<GetPermissionNamesByUserIdQuery, GetPermissionNamesByUserIdQueryResponse>
{
    public async Task<GetPermissionNamesByUserIdQueryResponse> HandleAsync(
        GetPermissionNamesByUserIdQuery query, CancellationToken cancellationToken)
    {
        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("Handling GetPermissionNamesByUserIdQuery for user {UserId}", query.UserId);

        var names = await permissionRepository.GetPermissionNamesByUserIdAsync(
            new UserId(query.UserId), cancellationToken);

        return new GetPermissionNamesByUserIdQueryResponse
        {
            Data = names,
            Message = "Permission names retrieved successfully."
        };
    }
}
