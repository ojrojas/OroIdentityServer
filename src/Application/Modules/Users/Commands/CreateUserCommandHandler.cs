// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using BuildingBlocks.Kernel.Persistence;
using OroIdentityServer.Core.Modules.Tenants.Entities;

namespace OroIdentityServer.Application.Modules.Users.Commands;

public class CreateUserCommandHandler(
    ILogger<CreateUserCommandHandler> logger,
    IUserRepository userRepository,
    ITenantRepository tenantRepository,
    IRepository<TenantUser> tenantUserRepository,
    IPasswordHasher passwordHasher)
: ICommandHandler<CreateUserCommand>
{
    public async Task<Result> HandleAsync(CreateUserCommand command, CancellationToken cancellationToken)
    {
        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("Handling CreateUserCommand for UserName: {UserName}", command.UserName);

        try
        {
            // Validate if user already exists
            var existingUser = await userRepository.GetUserByEmailAsync(command.Email, cancellationToken);
            if (existingUser is not null)
                return Result.Failure(Error.Conflict("UserAlreadyExists", "User with the given email already exists."));

            // Create the User object
            var user = User.Create(
                command.UserName,
                command.Email,
                command.Name,
                command.MiddleName,
                command.LastName,
                command.Identification,
              new(  command.IdentificationTypeId),
                new(command.TenantId)
            );

            // Assign SecurityUser
            user.AssignSecurityUser(SecurityUser.Create(
                await passwordHasher.HashPassword(command.Password)
            ));

            // Add the user to the repository
            await userRepository.AddUserAsync(user, cancellationToken);

            // Associate the new user with the tenant configured at creation time. Without a
            // TenantUser membership the user resolves to the Member role by default, losing
            // access to the admin console (403 on the dashboard/API).
            var tenant = await tenantRepository.GetByIdAsync(new(command.TenantId), cancellationToken);
            if (tenant is not null)
            {
                var membership = tenant.AddUser(user.Id, TenantRole.Member);
                await tenantUserRepository.AddAsync(membership, cancellationToken);
            }

            logger.LogInformation("Successfully handled CreateUserCommand for UserName: {UserName}", command.UserName);
            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while handling CreateUserCommand for UserName: {UserName}", command.UserName);
            throw;
        }
    }
}



