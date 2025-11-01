// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Services.OroIdentityServer.Application.Queries;

public class GetUserByEmailQueryHandler(
    ILogger<GetUserByEmailQueryHandler> logger,
    IUserRepository repository
) : IQueryHandler<GetUserByEmailQuery, User>
{
    public async Task<User> HandleAsync(GetUserByEmailQuery query, CancellationToken cancellationToken)
    {
        if(logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("Handling GetUserByEmail with Email: {Email}", query.Email);

        var user = await repository.GetUserByEmailAsync(query.Email);

        throw new NotImplementedException();
    }
}