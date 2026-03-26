// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Application.Commands;

public class CreatePermissionCommandHandler(
    ILogger<CreatePermissionCommandHandler> logger,
    IPermissionRepository permissionRepository)
: ICommandHandler<CreatePermissionCommand>
{
    public async Task HandleAsync(CreatePermissionCommand command, CancellationToken cancellationToken)
    {
        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("Handling CreatePermissionCommand for Name: {Name}", command.Name);

        try
        {
            var permission = Permission.Create(null, command.TenantId, command.Name, command.DisplayName, command.Description, command.Resource, command.IsSystem);
            await permissionRepository.AddPermissionAsync(permission, cancellationToken);
            logger.LogInformation("Successfully created permission with Name: {Name}", command.Name);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while handling CreatePermissionCommand for Name: {Name}", command.Name);
            throw;
        }
    }
}
