// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
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
        logger.LogInformation("Handling GetUsersQuery");
        GetUsersQueryResponse response = new()
        {
            Data = await repository.GetAllUsersAsync(cancellationToken),
            StatusCode = (int)HttpStatusCode.OK,
            Message = "Users retrieved successfully."
        };

        logger.LogInformation("Successfully handled GetUsersQuery");
        return response;
    }
}