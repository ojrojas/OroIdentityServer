// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using OroIdentityServer.Services.OroIdentityServer.Application.Queries;

/// <summary>
/// Handles the query to retrieve a user by their unique identifier.
/// </summary>
/// <remarks>
/// This query handler is responsible for processing <see cref="GetUserByIdQuery"/> 
/// and returning a <see cref="GetUserByIdQueryResponse"/> containing the user data.
/// </remarks>
/// <param name="logger">The logger instance used for logging information.</param>
/// <param name="repository">The repository instance used to access user data.</param>
/// <seealso cref="IQueryHandler{GetUserByIdQuery, GetUserByIdQueryResponse}"/>
public class GetUserByIdQueryHandler(
    Logger<GetUserByIdQueryHandler> logger, IUserRepository repository) 
    : IQueryHandler<GetUserByIdQuery, GetUserByIdQueryResponse>
{
    public async ValueTask<GetUserByIdQueryResponse> HandleAsync(GetUserByIdQuery query, CancellationToken cancellationToken)
    {
        GetUserByIdQueryResponse response = new();
        logger.LogInformation("Handling GetUserByIdQuery with Id: {Id}", query.Id.ToString());
        response.Data = await repository.GetAllUsersAsync();
        logger.LogInformation("Successfully handled GetUserByIdQuery");
        return response;
    }
}