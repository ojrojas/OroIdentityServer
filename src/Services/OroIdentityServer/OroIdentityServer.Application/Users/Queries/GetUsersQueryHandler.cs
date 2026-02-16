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
        var data = await repository.GetAllUsersAsync(cancellationToken);
        if(data == null || !data.Any())
        {
            logger.LogWarning("No users found in the repository");
            return new GetUsersQueryResponse
            {
                Data = null,
                StatusCode = (int)HttpStatusCode.NotFound,
                Message = "No users found."
            };
        }

        GetUsersQueryResponse response = new()
        {
            Data = data,
            StatusCode = (int)HttpStatusCode.OK,
            Message = "Users retrieved successfully."
        };

        logger.LogInformation("Successfully handled GetUsersQuery");
        return response;
    }
}