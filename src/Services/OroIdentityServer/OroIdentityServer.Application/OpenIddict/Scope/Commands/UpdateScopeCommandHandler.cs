// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.

namespace OroIdentityServer.Services.OroIdentityServer.Application.Commands;

public class UpdateScopeCommandHandler(
    ILogger<UpdateScopeCommandHandler> logger,
    IOpenIddictScopeManager scopeManager
) : ICommandHandler<UpdateScopeCommand>
{
    public async Task HandleAsync(UpdateScopeCommand command, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(command.Name))
            {
                logger.LogError("Scope name is null or empty. Cannot update scope.");
                throw new ArgumentException("Scope name cannot be null or empty.", nameof(command.Name));
            }

            var existingScope = await scopeManager.FindByNameAsync(command.Name, cancellationToken);

            if (existingScope == null)
            {
                logger.LogWarning("Scope with name {Name} not found.", command.Name);
                return;
            }

            var descriptor = new OpenIddictScopeDescriptor();
            descriptor.Resources.Clear();
            foreach (var resource in command.Resources)
            {
                descriptor.Resources.Add(resource);
            }

            await scopeManager.UpdateAsync(existingScope, descriptor, cancellationToken);

            logger.LogInformation("Scope with name {Name} updated successfully.", command.Name);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while updating the scope with name {Name}.", command.Name);
            throw;
        }
    }
}