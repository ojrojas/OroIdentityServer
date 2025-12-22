// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Services.OroIdentityServer.Application.Queries;

public class GetIdentificationTypesQueryHandler(
    ILogger<GetIdentificationTypesQueryHandler> logger,
    IIdentificationTypeRepository identificationTypeRepository
) : IQueryHandler<GetIdentificationTypesQuery, GetIdentificationTypesResponse>
{
    public async Task<GetIdentificationTypesResponse> HandleAsync(GetIdentificationTypesQuery query, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling GetIdentificationTypesQuery");

        try
        {
            var identificationTypes = await identificationTypeRepository.GetAllIdentificationTypesAsync(cancellationToken);

            logger.LogInformation("Successfully retrieved identification types");

            return new GetIdentificationTypesResponse
            {
                Data = identificationTypes,
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while handling GetIdentificationTypesQuery");

            return new GetIdentificationTypesResponse
            {
                Errors = ["An error occurred while retrieving identification types."]
            };
        }
    }
}