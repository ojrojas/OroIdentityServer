// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Application.Modules.Permissions.Queries;

public class GetPermissionByIdQueryHandler(
    ILogger<GetPermissionByIdQueryHandler> logger,
    IPermissionRepository permissionRepository)
: IQueryHandler<GetPermissionByIdQuery, GetPermissionByIdQueryResponse>
{
    public async Task<GetPermissionByIdQueryResponse> HandleAsync(GetPermissionByIdQuery query, CancellationToken cancellationToken)
    {
        var response = new GetPermissionByIdQueryResponse();
        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("Handling GetPermissionByIdQuery with Id: {Id}", query.PermissionId);

        try
        {
            response.Data = await permissionRepository.GetPermissionByIdAsync(query.PermissionId, cancellationToken);
            logger.LogInformation("Successfully handled GetPermissionByIdQuery for Id: {Id}", query.PermissionId);
            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while handling GetPermissionByIdQuery for Id: {Id}", query.PermissionId);
            throw;
        }
    }
}
