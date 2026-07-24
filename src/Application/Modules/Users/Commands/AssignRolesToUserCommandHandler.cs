namespace OroIdentityServer.Application.Modules.Users.Commands;

public sealed class AssignRolesToUserCommandHandler(
    ILogger<AssignRolesToUserCommandHandler> logger,
    IUserRepository userRepository,
    IUserRolesRepository userRolesRepository
) : ICommandHandler<AssignRolesToUserCommand>
{
    public async Task<Result> HandleAsync(AssignRolesToUserCommand command, CancellationToken cancellationToken)
    {
        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("Handling AssignRolesToUserCommand for UserId: {UserId}", command.UserId);

        try
        {
            var user = await userRepository.GetUserByIdAsync(new(command.UserId), cancellationToken) ?? throw new InvalidOperationException("User not found.");
            var currentRoles = (await userRolesRepository.GetRolesByUserIdAsync(new(command.UserId), cancellationToken)).ToList();

            var currentRoleIds = currentRoles.Select(r => r.RoleId!.Value).ToHashSet();
            var newRoleIds = command.RoleIds.ToHashSet();

            var toRemove = currentRoles.Where(r => r.RoleId is not null && !newRoleIds.Contains(r.RoleId.Value)).ToList();
            var toAdd = newRoleIds.Where(id => !currentRoleIds.Contains(id)).ToList();

            foreach (var role in toRemove)
            {
                user.RemoveRole(role);
                await userRolesRepository.DeleteUserRoleAsync(role, cancellationToken);
            }

            foreach (var roleId in toAdd)
            {
                var userRole = new UserRole(new(command.UserId), new(roleId));
                user.AddRole(userRole);
                await userRolesRepository.AddUserRoleAsync(userRole, cancellationToken);
            }

            await userRepository.UpdateUserAsync(user, cancellationToken);

            logger.LogInformation("Successfully assigned roles to UserId: {UserId}", command.UserId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while assigning roles to UserId: {UserId}", command.UserId);
            throw;
        }
    }
}
