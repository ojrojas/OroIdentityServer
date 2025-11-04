// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.

namespace OroIdentityServer.Services.OroIdentityServer.Application.Commands;

public class DeleteScopeCommandHandler(
    ILogger<DeleteScopeCommandHandler> logger,
    IOpenIddictScopeManager scopeManager
) : ICommandHandler<DeleteScopeCommand>
{
    public async Task HandleAsync(DeleteScopeCommand command, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(command.Name))
            {
                logger.LogError("Scope name is null or empty. Cannot delete scope.");
                throw new ArgumentException("Scope name cannot be null or empty.", nameof(command.Name));
            }

            var existingScope = await scopeManager.FindByNameAsync(command.Name, cancellationToken);

            if (existingScope == null)
            {
                logger.LogWarning("Scope with name {Name} not found.", command.Name);
                return;
            }

            await scopeManager.DeleteAsync(existingScope, cancellationToken);

            logger.LogInformation("Scope with name {Name} deleted successfully.", command.Name);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while deleting the scope with name {Name}.", command.Name);
            throw;
        }
    }
}