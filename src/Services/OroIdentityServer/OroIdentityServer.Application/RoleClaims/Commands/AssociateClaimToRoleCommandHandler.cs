// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.

namespace OroIdentityServer.Services.OroIdentityServer.Application.Commands;

public class AssociateClaimToRoleCommandHandler(IRolesRepository roleRepository, ILogger<AssociateClaimToRoleCommandHandler> logger) : ICommandHandler<AssociateClaimToRoleCommand>
{
    private readonly IRolesRepository _roleRepository = roleRepository;
    private readonly ILogger<AssociateClaimToRoleCommandHandler> _logger = logger;

    public async Task HandleAsync(AssociateClaimToRoleCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling AssociateClaimToRoleCommand for RoleId: {RoleId}, ClaimType: {ClaimType}", command.RoleId, command.ClaimType);

        try
        {
            var role = await _roleRepository.GetRoleByIdAsync(command.RoleId);

            if (role == null)
            {
                _logger.LogWarning("Role not found for Id: {RoleId}", command.RoleId);
                throw new KeyNotFoundException("Role not found.");
            }

            var roleClaim = new RoleClaim
            {
                RoleId = command.RoleId,
                ClaimType = command.ClaimType,
                ClaimValue = command.ClaimValue
            };

            await _roleRepository.AddRoleClaimAsync(roleClaim, cancellationToken);

            _logger.LogInformation("Successfully associated claim to role with RoleId: {RoleId}", command.RoleId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while associating the claim to the role with RoleId: {RoleId}", command.RoleId);
            throw;
        }
    }
}