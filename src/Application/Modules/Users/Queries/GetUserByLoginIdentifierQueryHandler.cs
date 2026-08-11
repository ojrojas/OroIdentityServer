namespace OroIdentityServer.Application.Modules.Users.Queries;

public class GetUserByLoginIdentifierQueryHandler(
    ILogger<GetUserByLoginIdentifierQueryHandler> logger,
    IUserRepository repository
) : IQueryHandler<GetUserByLoginIdentifierQuery, GetUserByLoginIdentifierResponse>
{
    public async Task<GetUserByLoginIdentifierResponse> HandleAsync(GetUserByLoginIdentifierQuery query, CancellationToken cancellationToken)
    {
        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("Handling GetUserByLoginIdentifier with LoginIdentifier: {LoginIdentifier}", query.LoginIdentifier);

        var user = await repository.GetUserByLoginIdentifierAsync(query.LoginIdentifier, cancellationToken);

        if (user is null)
        {
            logger.LogWarning("User not found with LoginIdentifier: {LoginIdentifier}", query.LoginIdentifier);
            return new GetUserByLoginIdentifierResponse
            {
                Data = null,
                StatusCode = (int)HttpStatusCode.NotFound,
                Message = "User not found."
            };
        }

        GetUserByLoginIdentifierResponse response = new()
        {
            Data = new UserDto
            {
                Id = user.Id.Value,
                Name = user.Name,
                LastName = user.LastName,
                MiddleName = user.MiddleName,
                UserName = user.UserName,
                Email = user.Email,
                Identification = user.Identification,
                IdentificationTypeId = user.IdentificationTypeId.Value,
                NormalizedEmail = user.NormalizedEmail,
                NormalizedUserName = user.NormalizedUserName,
                TenantId = user.TenantId.Value,
            }
        };

        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("Successful GetUserByLoginIdentifier with LoginIdentifier: {LoginIdentifier}", query.LoginIdentifier);

        return response;
    }
}
