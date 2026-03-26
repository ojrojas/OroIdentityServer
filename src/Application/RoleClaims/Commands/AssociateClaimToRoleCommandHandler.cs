// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Application.Commands;

public class AssociateClaimToRoleCommandHandler(
    ILogger<AssociateClaimToRoleCommandHandler> logger,
    IRolesRepository roleRepository) 
    : ICommandHandler<AssociateClaimToRoleCommand>
{
    private readonly IRolesRepository _roleRepository = roleRepository;
    private readonly ILogger<AssociateClaimToRoleCommandHandler> _logger = logger;

    public async Task HandleAsync(AssociateClaimToRoleCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling AssociateClaimToRoleCommand for RoleId: {RoleId}, ClaimType: {ClaimType}", command.RoleId, command.ClaimType);

        try
        {
            var role = await _roleRepository.GetRoleByIdAsync(command.RoleId, cancellationToken);

            if (role == null)
            {
                _logger.LogWarning("Role not found for Id: {RoleId}", command.RoleId);
                throw new KeyNotFoundException("Role not found.");
            }

            await _roleRepository.AddRoleClaimAsync(command.RoleId, command.ClaimType, command.ClaimValue, cancellationToken);

            _logger.LogInformation("Successfully associated claim to role with RoleId: {RoleId}", command.RoleId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while associating the claim to the role with RoleId: {RoleId}", command.RoleId);
            throw;
        }
    }
}