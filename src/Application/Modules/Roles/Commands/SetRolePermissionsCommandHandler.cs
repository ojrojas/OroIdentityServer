// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Application.Modules.Roles.Commands;

public class SetRolePermissionsCommandHandler(
    ILogger<SetRolePermissionsCommandHandler> logger,
    IRoleRepository roleRepository,
    IPermissionRepository permissionRepository) : ICommandHandler<SetRolePermissionsCommand>
{
    public async Task<Result> HandleAsync(SetRolePermissionsCommand command, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling SetRolePermissionsCommand for RoleId: {RoleId}", command.RoleId);

        try
        {
            var role = await roleRepository.GetWithPermissionsAsync(new RoleId(command.RoleId), cancellationToken);
            if (role is null)
            {
                logger.LogWarning("Role not found for Id: {RoleId}", command.RoleId);
                return Result.Failure(Error.NotFound("RoleNotFound", "Role not found."));
            }

            var requested = command.PermissionIds.Distinct().Select(id => new PermissionId(id)).ToHashSet();

            var allPermissions = (await permissionRepository.GetAllPermissionsAsync(cancellationToken)).ToList();
            var knownIds = allPermissions.Select(p => p.Id).ToHashSet();

            var unknown = requested.Where(id => !knownIds.Contains(id)).Select(id => id.Value).ToList();
            if (unknown.Count > 0)
            {
                logger.LogWarning("Unknown permission id(s) requested for role {RoleId}: {Ids}", command.RoleId, string.Join(", ", unknown));
                return Result.Failure(Error.Validation(
                    "PermissionNotFound",
                    $"Unknown permission id(s): {string.Join(", ", unknown)}"));
            }

            // Remove assignments no longer requested.
            foreach (var permissionId in role.RolePermissions.Select(rp => rp.PermissionId).ToList())
            {
                if (!requested.Contains(permissionId))
                    role.RemovePermission(permissionId);
            }

            // Add the missing assignments.
            var current = role.RolePermissions.Select(rp => rp.PermissionId).ToHashSet();
            foreach (var permission in allPermissions.Where(p => requested.Contains(p.Id) && !current.Contains(p.Id)))
            {
                role.AddPermission(permission);
            }

            await roleRepository.UpdateAsync(role, cancellationToken);

            logger.LogInformation("Successfully set {Count} permission(s) on role {RoleId}", requested.Count, command.RoleId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while setting permissions on role {RoleId}", command.RoleId);
            throw;
        }
    }
}
