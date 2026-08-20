// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Application.Modules.Users.Queries;

public class GetUsersByRoleAndTenantQueryHandler(
    ILogger<GetUsersByRoleAndTenantQueryHandler> logger,
    IUserRepository repository
) : IQueryHandler<GetUsersByRoleAndTenantQuery, GetUsersByRoleAndTenantQueryResponse>
{
    public async Task<GetUsersByRoleAndTenantQueryResponse> HandleAsync(GetUsersByRoleAndTenantQuery query, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling GetUsersByRoleAndTenantQuery for RoleId: {RoleId}, TenantId: {TenantId}", query.RoleId, query.TenantId);

        var data = await repository.GetUsersByRoleIdAsync(query.RoleId, query.TenantId, cancellationToken);

        GetUsersByRoleAndTenantQueryResponse response = new()
        {
            Data = data,
            StatusCode = (int)HttpStatusCode.OK,
            Message = "Users retrieved successfully."
        };

        logger.LogInformation("Successfully handled GetUsersByRoleAndTenantQuery");
        return response;
    }
}
