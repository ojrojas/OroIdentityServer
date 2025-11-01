// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.

namespace OroIdentityServer.Services.OroIdentityServer.Application.Queries;

public class GetUsersQueryHandler(
    ILogger<GetUserByEmailQueryHandler> logger, 
    IUserRepository repository
) : IQueryHandler<GetUsersQuery, GetUsersQueryResponse>
{
    public async Task<GetUsersQueryResponse> HandleAsync(GetUsersQuery query, CancellationToken cancellationToken)
    {
        GetUsersQueryResponse response = new();
        logger.LogInformation("Handling GetUsersQuery");
        response.Data = await repository.GetAllUsersAsync();
        logger.LogInformation("Successfully handled GetUserByIdQuery");
        return response;
    }
}