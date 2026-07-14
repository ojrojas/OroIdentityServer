namespace OroIdentityServer.Application.Modules.Users.Queries;

public class ValidateUserPasswordHandler(
    ILogger<ValidateUserPasswordHandler> logger,
    IUserRepository repository,
    ISecurityUserRepository securityUserRepository,
    IPasswordHasher passwordHasher
    ) : IQueryHandler<ValidateUserPasswordQuery, GetUserPasswordValidResponse>
{
    public async Task<GetUserPasswordValidResponse> HandleAsync(ValidateUserPasswordQuery query, CancellationToken cancellationToken)
    {
        logger.LogInformation("Validating password for user with login identifier: {LoginIdentifier}", query.LoginIdentifier);

        var user = await repository.GetUserByLoginIdentifierAsync(query.LoginIdentifier, cancellationToken);
        var securityUser = await securityUserRepository.GetSecurityUserAsync(user.SecurityUserId!.Value, cancellationToken);

        if (securityUser == null)
        {
            logger.LogWarning("User or SecurityUser not found for login identifier: {LoginIdentifier}", query.LoginIdentifier);
            return new GetUserPasswordValidResponse { Data = false };
        }

        var isPasswordValid = await passwordHasher.VerifyPassword(query.Password, securityUser.PasswordHash!);
        if (!isPasswordValid)
        {
            logger.LogWarning("Invalid password for user with login identifier: {LoginIdentifier}", query.LoginIdentifier);
            return new GetUserPasswordValidResponse { Data = false };
        }

        logger.LogInformation("Password validation successful for user with login identifier: {LoginIdentifier}", query.LoginIdentifier);
        return new GetUserPasswordValidResponse { Data = true };
    }
}
