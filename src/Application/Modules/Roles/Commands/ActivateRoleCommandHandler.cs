namespace OroIdentityServer.Application.Modules.Roles.Commands;

public class ActivateRoleCommandHandler(
    ILogger<ActivateRoleCommandHandler> logger,
    IRoleRepository roleRepository
) : ICommandHandler<ActivateRoleCommand>
{
    public async Task<Result> HandleAsync(ActivateRoleCommand command, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling ActivateRoleCommand for RoleId: {RoleId}", command.Id);

        try
        {
            var role = await roleRepository.GetByIdIgnoringFiltersAsync(new(command.Id), cancellationToken);
            if (role is null)
            {
                logger.LogWarning("Role not found with RoleId: {RoleId}", command.Id);
                return Result.Failure(Error.NotFound("RoleNotFound", "Role not found."));
            }

            if (role.IsActive)
            {
                return Result.Failure(Error.Conflict("RoleAlreadyActive", "Role is already active."));
            }

            role.Activate();
            await roleRepository.UpdateAsync(role, cancellationToken);

            logger.LogInformation("Successfully activated role {RoleId}", command.Id);
            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error activating role {RoleId}", command.Id);
            throw;
        }
    }
}
