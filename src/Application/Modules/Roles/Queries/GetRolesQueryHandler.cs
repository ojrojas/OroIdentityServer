// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Application.Modules.Roles.Queries;

public class GetRolesQueryHandler(
    ILogger<GetRolesQueryHandler> logger, IRoleRepository roleRepository
    )
    : IQueryHandler<GetRolesQuery, GetRolesResponse>
{
    public async Task<GetRolesResponse> HandleAsync(GetRolesQuery query, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling GetRolesQuery");

        try
        {
            var roles = await roleRepository.GetAllAsync(cancellationToken);

            logger.LogInformation("Successfully retrieved roles");

            return new GetRolesResponse
            {
                Data = roles.Select(r => new RoleDto
                (
                    r.Id.Value,
                     r.IsActive,
                     r.Name,
                     [.. r.RolePermissions.Select(rp => new RolePermissionDto(
                        rp.RoleId.Value, rp.PermissionId.Value))]
                )),
                StatusCode = 200,
                Message = "Roles retrieved successfully."
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while retrieving roles");

            return new GetRolesResponse
            {
                Errors = ["An error occurred while retrieving roles."],
                StatusCode = 500,
                Message = "An error occurred while retrieving roles."
            };
        }
    }
}