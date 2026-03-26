// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.

namespace OroIdentityServer.Application.Queries;

public class GetRoleByIdQueryHandler(
    ILogger<GetRoleByIdQueryHandler> logger,
    IRolesRepository roleRepository ) 
    : IQueryHandler<GetRoleByIdQuery, GetRoleByIdResponse>
{
    public async Task<GetRoleByIdResponse> HandleAsync(GetRoleByIdQuery query, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling GetRoleByIdQuery for RoleId: {RoleId}", query.Id);

        try
        {
            var role = await roleRepository.GetRoleByIdAsync(query.Id, cancellationToken);

            if (role == null)
            {
                logger.LogWarning("Role not found for Id: {RoleId}", query.Id);
                return new GetRoleByIdResponse
                {
                    Data = null,
                    Errors = ["Role not found."],
                    Message = $"Role with Id {query.Id} not found",
                    StatusCode = 404
                };
            }

            logger.LogInformation("Successfully retrieved role with Id: {RoleId}", query.Id);

            return new GetRoleByIdResponse
            {
                Data = role
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while retrieving the role with Id: {RoleId}", query.Id);

            return new GetRoleByIdResponse
            {
                Errors = ["An error occurred while retrieving the role."],
                StatusCode = 500,
                Message = $"An error occurred while retrieving the role with Id: {query.Id}"
            };
        }
    }
}