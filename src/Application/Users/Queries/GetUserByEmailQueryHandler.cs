// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Application.Queries;

public class GetUserByEmailQueryHandler(
    ILogger<GetUserByEmailQueryHandler> logger,
    IUserRepository repository
) : IQueryHandler<GetUserByEmailQuery, GetUserByEmailResponse>
{
    public async Task<GetUserByEmailResponse> HandleAsync(GetUserByEmailQuery query, CancellationToken cancellationToken)
    {
        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("Handling GetUserByEmail with Email: {Email}", query.Email);

        GetUserByEmailResponse response = new()
        {
            Data = await repository.GetUserByEmailAsync(query.Email, cancellationToken)
        };

        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("Successful GetUserByEmail with Email: {Email}", query.Email);

        return response;
    }
}