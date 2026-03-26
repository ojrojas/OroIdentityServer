// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.

namespace OroIdentityServer.Application.Commands;

public class AssociatePermissionsToRoleCommandHandler(
    ILogger<AssociatePermissionsToRoleCommandHandler> logger,
    IRolesRepository rolesRepository,
    IPermissionRepository permissionRepository
    ) : ICommandHandler<AssociatePermissionsToRoleCommand>
{
    private readonly ILogger<AssociatePermissionsToRoleCommandHandler> _logger = logger;
    private readonly IRolesRepository _rolesRepository = rolesRepository;
    private readonly IPermissionRepository _permissionRepository = permissionRepository;

    public async Task HandleAsync(AssociatePermissionsToRoleCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling AssociatePermissionsToRoleCommand for RoleId: {RoleId}, Count: {Count}", command.RoleId, command.PermissionIds?.Count() ?? 0);

        var role = await _rolesRepository.GetRoleByIdAsync(command.RoleId, cancellationToken);
        if (role == null)
        {
            _logger.LogWarning("Role not found for Id: {RoleId}", command.RoleId);
            throw new KeyNotFoundException("Role not found.");
        }

        // Load existing claims to prevent duplicates
        var existingClaims = await _rolesRepository.GetRoleClaimsByRoleIdAsync(command.RoleId, cancellationToken);
        var existingPermissionValues = new HashSet<string>(
            existingClaims
                .Where(rc => rc.ClaimType != null && rc.ClaimType.Value == "Permission")
                .Select(rc => rc.ClaimValue?.Value ?? string.Empty),
            StringComparer.OrdinalIgnoreCase);

        foreach (var pid in command.PermissionIds ?? Enumerable.Empty<PermissionId>())
        {
            try
            {
                var perm = await _permissionRepository.GetPermissionByIdAsync(pid, cancellationToken);
                if (perm == null) continue;

                if (existingPermissionValues.Contains(perm.Name))
                {
                    _logger.LogDebug("Permission {PermissionName} already assigned to role {RoleId}, skipping.", perm.Name, command.RoleId);
                    continue;
                }

                await _rolesRepository.AddRoleClaimAsync(command.RoleId, new RoleClaimType("Permission"), new RoleClaimValue(perm.Name), cancellationToken);
                existingPermissionValues.Add(perm.Name);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed assigning permission {PermissionId} to role {RoleId}", pid, command.RoleId);
            }
        }

        _logger.LogInformation("Finished AssociatePermissionsToRoleCommand for RoleId: {RoleId}", command.RoleId);
    }
}
