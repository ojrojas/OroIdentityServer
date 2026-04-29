namespace OroIdentityServer.Application.Modules.Roles.Queries;


public class GetRoleByNameQueryHandler(
    ILogger<GetRoleByNameQueryHandler> logger,
    IRoleRepository roleRepository
    ) : IQueryHandler<GetRoleByNameQuery, GetRoleByNameResponse>
{
    public Task<GetRoleByNameResponse> HandleAsync(GetRoleByNameQuery query, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling GetRoleByNameQuery for role name: {RoleName}", query.Name);

        try
        {
            var role = roleRepository.GetRoleByNameAsync(new(query.Name), cancellationToken).Result;

            if (role == null)
            {
                logger.LogWarning("Role with name {RoleName} not found", query.Name);
                return Task.FromResult(new GetRoleByNameResponse
                {
                    Data = null,
                    Errors = ["Role not found."],
                    Message = $"Role with name {query.Name} not found",
                    StatusCode = 404
                });
            }

            logger.LogInformation("Successfully retrieved role with name: {RoleName}", query.Name);

            return Task.FromResult(new GetRoleByNameResponse
            {
                Data = new RoleDto(
                  role.Id.Value,
                  role.IsActive,
                  role.Name,
                  role.RolePermissions.Select(
                    rp => new RolePermissionDto(rp.RoleId.Value, rp.PermissionId.Value))
                )
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while retrieving role with name: {RoleName}", query.Name);

            return Task.FromResult(new GetRoleByNameResponse
            {
                Data = null,
                Errors = ["An error occurred while retrieving the role."],
                Message = $"An error occurred while retrieving the role with name: {query.Name}",
                StatusCode = 500
            });
        }
    }
}