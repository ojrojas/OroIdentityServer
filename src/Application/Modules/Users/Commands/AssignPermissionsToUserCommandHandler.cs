// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Application.Modules.Users.Commands;

public sealed class AssignPermissionsToUserCommandHandler(
    ILogger<AssignPermissionsToUserCommandHandler> logger,
    IUserRepository userRepository,
    IUserPermissionsRepository userPermissionsRepository,
    IPermissionRepository permissionRepository
) : ICommandHandler<AssignPermissionsToUserCommand>
{
    public async Task<Result> HandleAsync(AssignPermissionsToUserCommand command, CancellationToken cancellationToken)
    {
        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("Handling AssignPermissionsToUserCommand for UserId: {UserId}", command.UserId);

        try
        {
            var user = await userRepository.GetUserByIdAsync(new(command.UserId), cancellationToken);
            if (user is null)
            {
                logger.LogWarning("User not found with UserId: {UserId}", command.UserId);
                return Result.Failure(Error.NotFound("UserNotFound", "User not found."));
            }

            var requested = command.PermissionIds.Distinct().Select(id => new PermissionId(id)).ToHashSet();

            var allPermissions = (await permissionRepository.GetAllPermissionsAsync(cancellationToken)).ToList();
            var knownIds = allPermissions.Select(p => p.Id).ToHashSet();

            var unknown = requested.Where(id => !knownIds.Contains(id)).Select(id => id.Value).ToList();
            if (unknown.Count > 0)
            {
                logger.LogWarning("Unknown permission id(s) requested for user {UserId}: {Ids}", command.UserId, string.Join(", ", unknown));
                return Result.Failure(Error.Validation(
                    "PermissionNotFound",
                    $"Unknown permission id(s): {string.Join(", ", unknown)}"));
            }

            var current = (await userPermissionsRepository.GetPermissionsByUserIdAsync(new(command.UserId), cancellationToken)).ToList();
            var currentIds = current.Select(p => p.PermissionId).ToHashSet();

            var toRemove = current.Where(p => !requested.Contains(p.PermissionId)).ToList();
            var toAdd = requested.Where(id => !currentIds.Contains(id)).ToList();

            foreach (var permission in toRemove)
            {
                user.RemovePermission(permission);
                await userPermissionsRepository.DeleteUserPermissionAsync(permission, cancellationToken);
            }

            foreach (var permissionId in toAdd)
            {
                var userPermission = new UserPermission(new(command.UserId), permissionId);
                user.AddPermission(userPermission);
                await userPermissionsRepository.AddUserPermissionAsync(userPermission, cancellationToken);
            }

            await userRepository.UpdateUserAsync(user, cancellationToken);

            logger.LogInformation("Successfully assigned {Count} direct permission(s) to UserId: {UserId}", requested.Count, command.UserId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while assigning permissions to UserId: {UserId}", command.UserId);
            throw;
        }
    }
}
