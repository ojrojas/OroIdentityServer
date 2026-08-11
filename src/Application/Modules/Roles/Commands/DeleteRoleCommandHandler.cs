// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Application.Modules.Roles.Commands;

public class DeleteRoleCommandHandler(
    IRoleRepository roleRepository,
    IUserRolesRepository userRolesRepository,
    ILogger<DeleteRoleCommandHandler> logger)
    : ICommandHandler<DeleteRoleCommand>
{
    private readonly IRoleRepository _roleRepository = roleRepository;
    private readonly IUserRolesRepository _userRolesRepository = userRolesRepository;
    private readonly ILogger<DeleteRoleCommandHandler> _logger = logger;

    public async Task<Result> HandleAsync(DeleteRoleCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling DeleteRoleCommand for RoleId: {RoleId}", command.Id);

        try
        {
            // Validate if role exists
            var role = await _roleRepository.GetByIdAsync(new(command.Id), cancellationToken);
            if (role is null)
            {
                _logger.LogWarning("Role not found with RoleId: {RoleId}", command.Id);
                return Result.Failure(Error.NotFound("RoleNotFound", "Role not found."));
            }

            // A role that is still assigned to users or still holds permissions cannot be deleted:
            // it would leave dangling references and break authorization.
            if (await _userRolesRepository.HasAnyForRoleAsync(new(command.Id), cancellationToken))
            {
                _logger.LogWarning("Role {RoleId} is assigned to users and cannot be deleted.", command.Id);
                return Result.Failure(Error.Conflict(
                    "RoleAssignedToUsers",
                    "The role is still assigned to one or more users and cannot be deleted."));
            }

            if (await _roleRepository.HasPermissionsAsync(new(command.Id), cancellationToken))
            {
                _logger.LogWarning("Role {RoleId} still has permissions assigned and cannot be deleted.", command.Id);
                return Result.Failure(Error.Conflict(
                    "RoleHasPermissions",
                    "The role still has permissions assigned and cannot be deleted."));
            }

            // Soft delete the role so existing references are preserved.
            role.Deactivate();
            await _roleRepository.UpdateAsync(role, cancellationToken);

            _logger.LogInformation("Successfully deleted role with Id: {RoleId}", command.Id);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while deleting the role with Id: {RoleId}", command.Id);
            throw;
        }
    }
}