// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Application.Modules.Openddict.Commands;

public class UpdateApplicationCommandHandler(
    ILogger<UpdateApplicationCommandHandler> logger,
    IOpenIddictApplicationManager applicationManager
) : ICommandHandler<UpdateApplicationCommand>
{
    public async Task<Result> HandleAsync(UpdateApplicationCommand command, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(command.Descriptor.ClientId))
            {
                logger.LogError("ClientId is null or empty. Cannot update application.");
                throw new ArgumentException("ClientId cannot be null or empty.", nameof(command.Descriptor.ClientId));
            }

            // Find the existing application
            var existingApplication = await applicationManager.FindByClientIdAsync(
                command.Descriptor.ClientId, cancellationToken);

            if (existingApplication == null)
            {
                logger.LogWarning("Application with ClientId {ClientId} not found.", command.Descriptor.ClientId);
                return Result.Success();
            }

            // Hydrate a descriptor with the values currently persisted (including the hashed
            // client secret, which is never exposed to callers) so unset fields are preserved.
            var existingDescriptor = new OpenIddictApplicationDescriptor();
            await applicationManager.PopulateAsync(existingDescriptor, existingApplication, cancellationToken);

            var descriptor = command.Descriptor.ToOpenIddict();

            descriptor.ClientType ??= existingDescriptor.ClientType;
            descriptor.ApplicationType ??= existingDescriptor.ApplicationType;
            descriptor.ConsentType ??= existingDescriptor.ConsentType;
            descriptor.JsonWebKeySet ??= existingDescriptor.JsonWebKeySet;

            // OpenIddict replaces the stored secret with the descriptor value, so the existing
            // hashed secret must be carried over when no new secret is supplied. Confidential
            // applications cannot be saved with a null secret unless they use client assertions.
            if (string.Equals(descriptor.ClientType, "public", StringComparison.OrdinalIgnoreCase))
            {
                descriptor.ClientSecret = null;
            }
            else if (string.IsNullOrWhiteSpace(descriptor.ClientSecret))
            {
                descriptor.ClientSecret = existingDescriptor.ClientSecret;
            }

            await applicationManager.UpdateAsync(existingApplication, descriptor, cancellationToken);

            logger.LogInformation("Application with ClientId {ClientId} updated successfully.", command.Descriptor.ClientId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while updating the application with ClientId {ClientId}.", command.Descriptor.ClientId);
            throw;
        }
    }
}