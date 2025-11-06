// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.

namespace OroIdentityServer.Services.OroIdentityServer.Application.Commands;

public class UpdateRoleCommandHandler(IRolesRepository roleRepository, ILogger<UpdateRoleCommandHandler> logger) : ICommandHandler<UpdateRoleCommand>
{
    private readonly IRolesRepository _roleRepository = roleRepository;
    private readonly ILogger<UpdateRoleCommandHandler> _logger = logger;

    public async Task HandleAsync(UpdateRoleCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling UpdateRoleCommand for RoleId: {RoleId}", command.Id);

        try
        {
            var role = await _roleRepository.GetRoleByIdAsync(command.Id);

            if (role == null)
            {
                _logger.LogWarning("Role not found for Id: {RoleId}", command.Id);
                throw new KeyNotFoundException("Role not found.");
            }

            role = new Role(command.RoleName)
            {
                Id = command.Id
            };

            await _roleRepository.UpdateRoleAsync(role, cancellationToken);

            _logger.LogInformation("Successfully updated role with Id: {RoleId}", command.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while updating the role with Id: {RoleId}", command.Id);
            throw;
        }
    }
}