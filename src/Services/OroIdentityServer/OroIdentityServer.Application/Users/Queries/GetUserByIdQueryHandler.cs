// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Application.Queries;

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
    ILogger<GetUserByIdQueryHandler> logger, IUserRepository repository) 
    : IQueryHandler<GetUserByIdQuery, GetUserByIdQueryResponse>
{
    public async Task<GetUserByIdQueryResponse> HandleAsync(GetUserByIdQuery query, CancellationToken cancellationToken)
    {
        GetUserByIdQueryResponse response = new();
        logger.LogInformation("Handling GetUserByIdQuery with Id: {Id}", query.Id.ToString());
        response.Data = await repository.GetUserByIdAsync(query.Id, cancellationToken);
        logger.LogInformation("Successfully handled GetUserByIdQuery");
        return response;
    }
}