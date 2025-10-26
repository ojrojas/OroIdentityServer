// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Services.OroIdentityServer.Application.Commands;

/// <summary>
/// Handles the creation of a new role by processing the <see cref="CreateRoleCommand"/>.
/// </summary>
/// <remarks>
/// This command handler is responsible for creating a new role and saving it to the repository.
/// </remarks>
/// <param name="roleRepository">The repository used to manage roles.</param>
public class CreateRoleCommandHandler(IRolesRepository roleRepository) : ICommandHandler<CreateRoleCommand>
{
    private readonly IRolesRepository _roleRepository = roleRepository;

    public async Task HandleAsync(CreateRoleCommand command, CancellationToken cancellationToken)
    {
        var role = new Role(new RoleName(command.RoleName));
        await _roleRepository.AddRoleAsync(role, cancellationToken);
    }
}