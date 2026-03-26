// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.

namespace OroIdentityServer.Application.Queries;

public class GetIdentificationTypeByIdQueryHandler(
    ILogger<GetIdentificationTypeByIdQueryHandler> logger,
    IIdentificationTypeRepository identificationTypeRepository
) : IQueryHandler<GetIdentificationTypeByIdQuery, GetIdentificationTypeByIdResponse>
{
    public async Task<GetIdentificationTypeByIdResponse> HandleAsync(
        GetIdentificationTypeByIdQuery query, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling GetIdentificationTypeByIdQuery for Id: {Id}", query.Id);

        try
        {
            var identificationType = await identificationTypeRepository.GetIdentificationTypeByIdAsync(
                query.Id, cancellationToken);

            if (identificationType == null)
            {
                logger.LogWarning("IdentificationType not found for Id: {Id}", query.Id);
                return new GetIdentificationTypeByIdResponse
                {
                    Data = null,
                    StatusCode = 500,

                    Errors = ["IdentificationType not found."]
                };
            }

            logger.LogInformation("Successfully retrieved IdentificationType for Id: {Id}", query.Id);

            return new GetIdentificationTypeByIdResponse
            {
                Data = identificationType,
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while handling GetIdentificationTypeByIdQuery for Id: {Id}", query.Id);

            return new GetIdentificationTypeByIdResponse
            {
                StatusCode = 500,
                Data = null, 
                Errors = ["An error occurred while retrieving the IdentificationType."]
            };
        }
    }
}