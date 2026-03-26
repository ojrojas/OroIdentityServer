// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Application.Commands;

public class DeletePermissionCommandHandler(
    ILogger<DeletePermissionCommandHandler> logger,
    IPermissionRepository permissionRepository)
: ICommandHandler<DeletePermissionCommand>
{
    public async Task HandleAsync(DeletePermissionCommand command, CancellationToken cancellationToken)
    {
        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("Handling DeletePermissionCommand for PermissionId: {Id}", command.PermissionId);

        try
        {
            var permission = await permissionRepository.GetPermissionByIdAsync(command.PermissionId, cancellationToken);
            if (permission == null)
                throw new InvalidOperationException("Permission not found.");

            await permissionRepository.DeletePermissionAsync(command.PermissionId, cancellationToken);

            logger.LogInformation("Successfully deleted permission with Id: {Id}", command.PermissionId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while deleting permission with Id: {Id}", command.PermissionId);
            throw;
        }
    }
}
