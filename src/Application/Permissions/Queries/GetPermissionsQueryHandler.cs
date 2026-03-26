// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Application.Queries;

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

        try
        {
            response.Data = await permissionRepository.GetAllPermissionsAsync(cancellationToken);
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
