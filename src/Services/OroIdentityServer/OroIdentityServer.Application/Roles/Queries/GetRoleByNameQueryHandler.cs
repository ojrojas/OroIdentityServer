public class GetRoleByNameQueryHandler(
    ILogger<GetRoleByNameQueryHandler> logger,
    IRolesRepository roleRepository
    )  : IQueryHandler<GetRoleByNameQuery, GetRoleByNameResponse>
{
    public Task<GetRoleByNameResponse> HandleAsync(GetRoleByNameQuery query, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling GetRoleByNameQuery for role name: {RoleName}", query.Name);

        try
        {
            var role = roleRepository.GetRoleByNameAsync(query.Name, cancellationToken).Result;

            if (role == null)
            {
                logger.LogWarning("Role with name {RoleName} not found", query.Name);
                return Task.FromResult(new GetRoleByNameResponse
                {
                    Data = null,
                    Errors = ["Role not found."]
                });
            }

            logger.LogInformation("Successfully retrieved role with name: {RoleName}", query.Name);

            return Task.FromResult(new GetRoleByNameResponse
            {
                Data = role,
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while retrieving role with name: {RoleName}", query.Name);

            return Task.FromResult(new GetRoleByNameResponse
            {
                Data = null,
                Errors = ["An error occurred while retrieving the role."]
            });
        }
    }
}