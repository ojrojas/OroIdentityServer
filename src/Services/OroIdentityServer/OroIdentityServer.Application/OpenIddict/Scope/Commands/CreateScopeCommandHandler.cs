// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.

namespace OroIdentityServer.Services.OroIdentityServer.Application.Commands;

public class CreateScopeCommandHandler(
    ILogger<CreateScopeCommandHandler> logger,
    IOpenIddictScopeManager scopeManager
) : ICommandHandler<CreateScopeCommand>
{
    public async Task HandleAsync(CreateScopeCommand command, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(command.Name))
            {
                logger.LogError("Scope name is null or empty. Cannot create scope.");
                throw new ArgumentException("Scope name cannot be null or empty.", nameof(command.Name));
            }

            var existingScope = await scopeManager.FindByNameAsync(command.Name, cancellationToken);

            if (existingScope != null)
            {
                logger.LogWarning("Scope with name {Name} already exists.", command.Name);
                return;
            }

            var descriptor = new OpenIddictScopeDescriptor
            {
                Name = command.Name,
            };

            foreach(var res in command.Resources)
            {
                descriptor.Resources.Add(res);    
                logger.LogInformation("Added resoure {res}", res);
            }

            await scopeManager.CreateAsync(descriptor, cancellationToken);

            logger.LogInformation("Scope with name {Name} created successfully.", command.Name);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while creating the scope with name {Name}.", command.Name);
            throw;
        }
    }
}