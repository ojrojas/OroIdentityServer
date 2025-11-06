// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.

namespace OroIdentityServer.Services.OroIdentityServer.Application.Commands;

public class UpdateRoleClaimCommandHandler(IRolesRepository roleRepository, ILogger<UpdateRoleClaimCommandHandler> logger) : ICommandHandler<UpdateRoleClaimCommand>
{
    private readonly IRolesRepository _roleRepository = roleRepository;
    private readonly ILogger<UpdateRoleClaimCommandHandler> _logger = logger;

    public async Task HandleAsync(UpdateRoleClaimCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling UpdateRoleClaimCommand for RoleClaimId: {RoleClaimId}", command.RoleClaimId);

        try
        {
            var roleClaim = await _roleRepository.GetRoleClaimByIdAsync(command.RoleClaimId);

            if (roleClaim == null)
            {
                _logger.LogWarning("RoleClaim not found for Id: {RoleClaimId}", command.RoleClaimId);
                throw new KeyNotFoundException("RoleClaim not found.");
            }

            roleClaim.ClaimType = command.ClaimType;
            roleClaim.ClaimValue = command.ClaimValue;

            await _roleRepository.UpdateRoleClaimAsync(roleClaim, cancellationToken);

            _logger.LogInformation("Successfully updated RoleClaim with Id: {RoleClaimId}", command.RoleClaimId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while updating the RoleClaim with Id: {RoleClaimId}", command.RoleClaimId);
            throw;
        }
    }
}