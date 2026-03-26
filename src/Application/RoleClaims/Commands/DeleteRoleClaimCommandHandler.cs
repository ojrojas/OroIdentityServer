// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.

namespace OroIdentityServer.Application.Commands;

public class DeleteRoleClaimCommandHandler(ILogger<DeleteRoleClaimCommandHandler> logger, IRolesRepository roleRepository) : ICommandHandler<DeleteRoleClaimCommand>
{
    private readonly ILogger<DeleteRoleClaimCommandHandler> _logger = logger;
    private readonly IRolesRepository _roleRepository = roleRepository;

    public async Task HandleAsync(DeleteRoleClaimCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling DeleteRoleClaimCommand for RoleClaimId: {RoleClaimId}", command.RoleClaimId);

        try
        {
            var roleClaim = await _roleRepository.GetRoleClaimByIdAsync(command.RoleClaimId.Value, cancellationToken);

            if (roleClaim == null)
            {
                _logger.LogWarning("RoleClaim not found for Id: {RoleClaimId}", command.RoleClaimId);
                throw new KeyNotFoundException("RoleClaim not found.");
            }

            await _roleRepository.DeleteRoleClaimAsync(roleClaim.Id, cancellationToken);

            _logger.LogInformation("Successfully deleted RoleClaim with Id: {RoleClaimId}", command.RoleClaimId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while deleting the RoleClaim with Id: {RoleClaimId}", command.RoleClaimId);
            throw;
        }
    }
}