// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Application.Modules.Permissions.Queries;

public class GetPermissionsQueryHandler(
    ILogger<GetPermissionsQueryHandler> logger,
    IPermissionRepository permissionRepository)
: IQueryHandler<GetPermissionsQuery, GetPermissionsQueryResponse>
{
    public async Task<GetPermissionsQueryResponse> HandleAsync(GetPermissionsQuery query, CancellationToken cancellationToken)
    {
        var response = new GetPermissionsQueryResponse();
        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("Handling GetPermissionsQuery");

        var permissions = await permissionRepository.GetAllPermissionsAsync(cancellationToken);

        try
        {
            response.Data = permissions.Select(p => new PermissionDto(
                p.Id.Value, 
                p.Name,
                p.Description,
                p.Action, 
                p.Resource, 
                p.IsSystem));
            logger.LogInformation("Successfully handled GetPermissionsQuery");
            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while handling GetPermissionsQuery");
            throw;
        }
    }
}
