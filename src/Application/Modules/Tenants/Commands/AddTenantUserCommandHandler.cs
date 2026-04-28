// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Application.Modules.Tenants.Commands;

public sealed class AddTenantUserCommandHandler(
    ILogger<AddTenantUserCommandHandler> logger,
    ITenantRepository tenantRepository)
    : ICommandHandler<AddTenantUserCommand>
{
    public async Task HandleAsync(AddTenantUserCommand command, CancellationToken ct)
    {
        logger.LogInformation("Handling AddTenantUserCommand for TenantId: {TenantId}, UserId: {UserId}",
            command.TenantId, command.UserId);

        try
        {
            var tenant = await tenantRepository.GetByIdAsync(TenantId.From(command.TenantId), ct)
                ?? throw new InvalidOperationException($"Tenant '{command.TenantId}' not found.");

            tenant.AddUser(UserId.From(command.UserId), command.Role);

            await tenantRepository.UpdateAsync(tenant, ct);

            logger.LogInformation("Successfully added user {UserId} to tenant {TenantId}",
                command.UserId, command.TenantId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error adding user {UserId} to tenant {TenantId}",
                command.UserId, command.TenantId);
            throw;
        }
    }
}
