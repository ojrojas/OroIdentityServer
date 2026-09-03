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
            var allRoles = await roleRepository.GetAllAsync(cancellationToken);
            var rolesList = allRoles.ToList();

            if (!string.IsNullOrWhiteSpace(query.SearchTerm))
            {
                var term = query.SearchTerm.Trim();
                rolesList = rolesList.Where(r =>
                    r.Name != null && r.Name.Value.Contains(term, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            var totalCount = rolesList.Count;
            var skip = (query.PageNumber - 1) * query.PageSize;
            var pagedRoles = rolesList.Skip(skip).Take(query.PageSize).ToList();

            logger.LogInformation("Successfully retrieved roles");

            return new GetRolesResponse
            {
                Data = pagedRoles.Select(r => new RoleDto
                (
                    r.Id.Value,
                     r.IsActive,
                     r.Name,
                     [.. r.RolePermissions.Select(rp => new RolePermissionDto(
                        rp.RoleId.Value, rp.PermissionId.Value))],
                     r.CreatedAtUtc,
                     r.Level,
                     r.ParentRoleId?.Value
                )),
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize,
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