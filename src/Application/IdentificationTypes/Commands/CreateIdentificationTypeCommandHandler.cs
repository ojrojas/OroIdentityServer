// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Application.Commands;

/// <summary>
/// Handles the creation of a new IdentificationType.
/// </summary>
/// <remarks>
/// This handler processes the <see cref="CreateIdentificationTypeCommand"/> to create a new
/// IdentificationType entity and persist it in the repository. It logs the process at various stages
/// and handles any exceptions that may occur during the operation.
/// </remarks>
/// <param name="logger">The logger instance used to log information, debug messages, and errors.</param>
/// <param name="repository">The repository instance used to persist the IdentificationType entity.</param>
/// <seealso cref="CreateIdentificationTypeCommand"/>
public class CreateIdentificationTypeCommandHandler(
    ILogger<CreateIdentificationTypeCommandHandler> logger,
    IIdentificationTypeRepository repository
) : ICommandHandler<CreateIdentificationTypeCommand>
{
    public async Task HandleAsync(CreateIdentificationTypeCommand command, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling CreateIdentificationTypeCommand for IdentificationTypeName: {IdentificationTypeName}", command.Name);

        try
        {
            // Create the IdentificationType object
            logger.LogDebug("Creating IdentificationType object for Name: {IdentificationTypeName}", command.Name);
            var identificationType = new IdentificationType(command.Name.Value);

            // Add the IdentificationType to the repository
            logger.LogDebug("Adding IdentificationType to repository for Name: {IdentificationTypeName}", command.Name);
            await repository.AddIdentificationTypeAsync(identificationType, cancellationToken);

            logger.LogInformation("Successfully handled CreateIdentificationTypeCommand for IdentificationTypeName: {IdentificationTypeName}", command.Name);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while handling CreateIdentificationTypeCommand for IdentificationTypeName: {IdentificationTypeName}", command.Name);
            throw;
        }
    }
}