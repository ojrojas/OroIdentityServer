// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Application.Queries;

public class ValidateUserToLoginQueryHandler(
    ILogger<ValidateUserToLoginQuery> logger,
    IUserRepository userRepository
) : IQueryHandler<ValidateUserToLoginQuery, bool>
{
    public async Task<bool> HandleAsync(ValidateUserToLoginQuery query, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling ValidateUserToLoginQuery for email: {Email}", query.Email);

        if (string.IsNullOrWhiteSpace(query.Email))
        {
            logger.LogWarning("Validation failed: Email is null or empty.");
            return false;
        }

        try
        {
            var canLogin = await userRepository.ValidateUserCanLoginAsync(query.Email, cancellationToken);

            if (!canLogin)
            {
                logger.LogWarning("User with email {Email} cannot login due to lockout or other restrictions.", query.Email);
            }
            else
            {
                logger.LogInformation("User with email {Email} is allowed to login.", query.Email);
            }

            return canLogin;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while validating user login for email: {Email}", query.Email);
            throw;
        }
    }
}