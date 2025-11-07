// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Application.Queries;

public class ValidateUserToLoginQueryHandler(
    ILogger<ValidateUserToLoginQuery> logger,
    IUserRepository userRepository
) : IQueryHandler<ValidateUserToLoginQuery, bool>
{


    public async Task<bool> Handle(ValidateUserToLoginQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling ValidateUserToLoginQuery for email: {Email}", request.Email);

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            logger.LogWarning("Validation failed: Email is null or empty.");
            return false;
        }

        try
        {
            var canLogin = await userRepository.ValidateUserCanLoginAsync(request.Email, cancellationToken);

            if (!canLogin)
            {
                logger.LogWarning("User with email {Email} cannot login due to lockout or other restrictions.", request.Email);
            }
            else
            {
                logger.LogInformation("User with email {Email} is allowed to login.", request.Email);
            }

            return canLogin;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while validating user login for email: {Email}", request.Email);
            throw;
        }
    }

    public Task<bool> HandleAsync(ValidateUserToLoginQuery query, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}