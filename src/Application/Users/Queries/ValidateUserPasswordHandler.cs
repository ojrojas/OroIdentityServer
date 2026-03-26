// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.

namespace OroIdentityServer.Application.Queries;

public class ValidateUserPasswordHandler(
    ILogger<ValidateUserPasswordHandler> logger,
    IUserRepository repository,
    ISecurityUserRepository securityUserRepository,
    IPasswordHasher passwordHasher
    ) : IQueryHandler<ValidateUserPasswordQuery, GetUserPasswordValidResponse>
{
    public async Task<GetUserPasswordValidResponse> HandleAsync(ValidateUserPasswordQuery query, CancellationToken cancellationToken)
    {
        logger.LogInformation("Validating password for user with email: {Email}", query.Email);

        // Retrieve the user by email
        var user = await repository.GetUserByEmailAsync(query.Email, cancellationToken);
        var securityUser = await securityUserRepository.GetSecurityUserAsync(user.SecurityUserId!.Value, cancellationToken);

        if (securityUser == null)
        {
            logger.LogWarning("User or SecurityUser not found for email: {Email}", query.Email);
            return new GetUserPasswordValidResponse { Data = false };
        }

        // Validate the password using the password hasher
        var isPasswordValid = await passwordHasher.VerifyPassword(query.Password, securityUser.PasswordHash!);
        if (!isPasswordValid)
        {
            logger.LogWarning("Invalid password for user with email: {Email}", query.Email);
            return new GetUserPasswordValidResponse { Data = false };
        }

        logger.LogInformation("Password validation successful for user with email: {Email}", query.Email);
        return new GetUserPasswordValidResponse { Data = true };
    }
}