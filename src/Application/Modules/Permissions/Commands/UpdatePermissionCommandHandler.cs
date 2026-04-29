// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Application.Modules.Permissions.Commands;

public class UpdatePermissionCommandHandler(
    ILogger<UpdatePermissionCommandHandler> logger,
    IPermissionRepository permissionRepository)
: ICommandHandler<UpdatePermissionCommand>
{
    public async Task HandleAsync(UpdatePermissionCommand command, CancellationToken cancellationToken)
    {
        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("Handling UpdatePermissionCommand for PermissionId: {Id}", command.PermissionId);

        try
        {
            var permission = await permissionRepository.GetPermissionByIdAsync(new(command.PermissionId), cancellationToken);
            if (permission == null)
                throw new InvalidOperationException("Permission not found.");

            permission.Update(command.Provider, command.Description, command.Action, command.Resource, command.IsSystem);
            await permissionRepository.UpdatePermissionAsync(permission, cancellationToken);

            logger.LogInformation("Successfully updated permission with Id: {Id}", command.PermissionId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while updating permission with Id: {Id}", command.PermissionId);
            throw;
        }
    }
}
