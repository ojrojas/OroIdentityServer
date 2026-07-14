namespace OroIdentityServer.Application.Modules.Users.Queries;

public class ValidateUserToLoginQueryHandler(
    ILogger<ValidateUserToLoginQuery> logger,
    IUserRepository userRepository
) : IQueryHandler<ValidateUserToLoginQuery, bool>
{
    public async Task<bool> HandleAsync(ValidateUserToLoginQuery query, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling ValidateUserToLoginQuery for login identifier: {LoginIdentifier}", query.LoginIdentifier);

        if (string.IsNullOrWhiteSpace(query.LoginIdentifier))
        {
            logger.LogWarning("Validation failed: LoginIdentifier is null or empty.");
            return false;
        }

        try
        {
            var canLogin = await userRepository.ValidateUserCanLoginAsync(query.LoginIdentifier, cancellationToken);

            if (!canLogin)
            {
                logger.LogWarning("User with login identifier {LoginIdentifier} cannot login due to lockout or other restrictions.", query.LoginIdentifier);
            }
            else
            {
                logger.LogInformation("User with login identifier {LoginIdentifier} is allowed to login.", query.LoginIdentifier);
            }

            return canLogin;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while validating user login for identifier: {LoginIdentifier}", query.LoginIdentifier);
            throw;
        }
    }
}
