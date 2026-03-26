// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Application.Commands;

public class UpdateIdentificationTypeCommandHandler(
    ILogger<UpdateIdentificationTypeCommandHandler> logger,
    IIdentificationTypeRepository repository
) : ICommandHandler<UpdateIdentificationTypeCommand>
{
    public async Task HandleAsync(UpdateIdentificationTypeCommand command, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Starting update process for IdentificationType with ID {Id}", command.Id);

            var identificationTypeExist = await repository.GetIdentificationTypeByIdAsync(command.Id, cancellationToken);
            if (identificationTypeExist == null)
            {
                logger.LogWarning("IdentificationType with ID {Id} not found", command.Id);
                throw new KeyNotFoundException($"IdentificationType with ID {command.Id} not found.");
            }

            identificationTypeExist.UpdateName(command.Name);

            await repository.UpdateIdentificationTypeAsync(identificationTypeExist, cancellationToken);

            logger.LogInformation("Successfully updated IdentificationType with ID {Id}", command.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while updating IdentificationType with ID {Id}", command.Id);
            throw;
        }
    }
}