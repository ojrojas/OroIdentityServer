// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.

namespace OroIdentityServer.Application.Commands;

public class UpdateRoleClaimCommandHandler(
    ILogger<UpdateRoleClaimCommandHandler> logger,
    IRolesRepository roleRepository
    ) : ICommandHandler<UpdateRoleClaimCommand>
{
    public async Task HandleAsync(UpdateRoleClaimCommand command, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling UpdateRoleClaimCommand for RoleClaimId: {RoleClaimId}", command.RoleClaimId);

        try
        {
            var roleClaim = await roleRepository.GetRoleClaimByIdAsync(command.RoleClaimId.Value, cancellationToken);

            if (roleClaim == null)
            {
                logger.LogWarning("RoleClaim not found for Id: {RoleClaimId}", command.RoleClaimId);
                throw new KeyNotFoundException("RoleClaim not found.");
            }

            await roleRepository.UpdateRoleClaimAsync(command.RoleClaimId.Value, new RoleClaimType(command.ClaimType), new RoleClaimValue(command.ClaimValue), cancellationToken);

            logger.LogInformation("Successfully updated RoleClaim with Id: {RoleClaimId}", command.RoleClaimId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while updating the RoleClaim with Id: {RoleClaimId}", command.RoleClaimId);
            throw;
        }
    }
}