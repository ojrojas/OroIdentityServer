// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Application.Commands;

/// <summary>
/// Handles the creation of a new role by processing the <see cref="CreateRoleCommand"/>.
/// </summary>
/// <remarks>
/// This command handler is responsible for creating a new role and saving it to the repository.
/// </remarks>
/// <param name="roleRepository">The repository used to manage roles.</param>
public class CreateRoleCommandHandler(
    ILogger<CreateRoleCommandHandler> logger, IRolesRepository roleRepository
    ) : ICommandHandler<CreateRoleCommand>
{
    public async Task HandleAsync(CreateRoleCommand command, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling CreateRoleCommand for RoleName: {RoleName}", command.RoleName);

        try
        {
            // Validate if role already exists
            var existingRole = await roleRepository.GetRoleByNameAsync(command.RoleName.Value, cancellationToken);
            if (existingRole != null)
                throw new InvalidOperationException("Role with the given name already exists.");

            // Create the Role object
            var role = new Role(command.RoleName.Value);

            // Add the role to the repository
            await roleRepository.AddRoleAsync(role, cancellationToken);

            logger.LogInformation("Successfully created role with RoleName: {RoleName}", command.RoleName);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while creating the role with RoleName: {RoleName}", command.RoleName);
            throw;
        }
    }
}