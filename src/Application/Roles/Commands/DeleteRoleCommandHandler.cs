// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.

namespace OroIdentityServer.Application.Commands;

public class DeleteRoleCommandHandler(IRolesRepository roleRepository, ILogger<DeleteRoleCommandHandler> logger) : ICommandHandler<DeleteRoleCommand>
{
    private readonly IRolesRepository _roleRepository = roleRepository;
    private readonly ILogger<DeleteRoleCommandHandler> _logger = logger;

    public async Task HandleAsync(DeleteRoleCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling DeleteRoleCommand for RoleId: {RoleId}", command.Id);

        try
        {
            // Validate if role exists
            var role = await _roleRepository.GetRoleByIdAsync(command.Id, cancellationToken);
            if (role == null)
                throw new InvalidOperationException("Role not found.");

            // Delete the role
            await _roleRepository.DeleteRoleAsync(command.Id, cancellationToken);

            // Raise domain event
            role.RaiseDomainEvent(new RoleDeletedEvent(command.Id));

            _logger.LogInformation("Successfully deleted role with Id: {RoleId}", command.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while deleting the role with Id: {RoleId}", command.Id);
            throw;
        }
    }
}