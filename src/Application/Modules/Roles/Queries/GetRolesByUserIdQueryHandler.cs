// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Application.Modules.Roles.Queries;

public sealed class GetRolesByUserIdQueryHandler(
    ILogger<GetRolesByUserIdQueryHandler> logger,
    IRoleRepository roleRepository) : IQueryHandler<GetRolesByUserIdQuery, GetRolesResponse>
{
    public async Task<GetRolesResponse> HandleAsync(GetRolesByUserIdQuery query, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling GetRolesByUserIdQuery for user id: {Id}", query.UserId);
        try
        {
            var roles = await roleRepository.GetRolesByUserIdAsync(new(query.UserId), cancellationToken);

            logger.LogInformation("Successfully retrieved roles for user id: {Id}", query.UserId);

            return new GetRolesResponse
            {
                Data = roles.Select(r => new RoleDto(
                    r.Id.Value,
                    r.IsActive,
                    r.Name,
                    [.. r.RolePermissions.Select(rp => new RolePermissionDto(rp.RoleId.Value, rp.PermissionId.Value))],
                    r.CreatedAtUtc
                )),
                StatusCode = 200,
                Message = "Roles retrieved successfully."
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while retrieving roles for user id: {Id}", query.UserId);

            return new GetRolesResponse
            {
                Errors = ["An error occurred while retrieving roles for the user."],
                StatusCode = 500,
                Message = $"An error occurred while retrieving roles for user id: {query.UserId}"
            };
        }
    }
}