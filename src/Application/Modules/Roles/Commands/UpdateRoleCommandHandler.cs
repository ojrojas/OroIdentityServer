// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Application.Modules.Roles.Commands;

public class UpdateRoleCommandHandler(
    ILogger<UpdateRoleCommandHandler> logger,
    IRoleRepository roleRepository
    ) : ICommandHandler<UpdateRoleCommand>
{
    public async Task HandleAsync(UpdateRoleCommand command, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling UpdateRoleCommand for RoleId: {RoleId}", command.Id);

        try
        {
            var role = await roleRepository.GetByIdAsync(new(command.Id), cancellationToken);

            if (role == null)
            {
                logger.LogWarning("Role not found for Id: {RoleId}", command.Id);
                throw new KeyNotFoundException("Role not found.");
            }

            role.UpdateName(new (command.RoleName));
            await roleRepository.UpdateAsync(role, cancellationToken);

            logger.LogInformation("Successfully updated role with Id: {RoleId}", command.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while updating the role with Id: {RoleId}", command.Id);
            throw;
        }
    }
}