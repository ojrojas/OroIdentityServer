// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Application.Commands;

public class DeleteIdentificationTypeCommandHandler(
    ILogger<DeleteIdentificationTypeCommandHandler> logger,
    IIdentificationTypeRepository repository
) : ICommandHandler<DeleteIdentificationTypeCommand>
{
    public async Task HandleAsync(DeleteIdentificationTypeCommand command, CancellationToken cancellationToken)
    {
         try
        {
            logger.LogInformation("Starting delete process for IdentificationType with ID {Id}", command.Id);

            var identificationTypeExist = await repository.GetIdentificationTypeByIdAsync(command.Id, cancellationToken);
            if (identificationTypeExist == null)
            {
                logger.LogWarning("IdentificationType with ID {Id} not found", command.Id);
                throw new KeyNotFoundException($"IdentificationType with ID {command.Id} not found.");
            }

            await repository.DeleteIdentificationTypeAsync(command.Id, cancellationToken);

            logger.LogInformation("Successfully deleted IdentificationType with ID {Id}", command.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while deleting IdentificationType with ID {Id}", command.Id);
            throw;
        }
    }
}