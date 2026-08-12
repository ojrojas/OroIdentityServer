// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using BuildingBlocks.Kernel.Persistence;
using OroIdentityServer.Core.Modules.Tenants.Entities;

namespace OroIdentityServer.Application.Modules.Tenants.Commands;

public sealed class AddTenantUserCommandHandler(
    ILogger<AddTenantUserCommandHandler> logger,
    ITenantRepository tenantRepository,
    IRepository<TenantUser> tenantUserRepository)
    : ICommandHandler<AddTenantUserCommand>
{
    public async Task<Result> HandleAsync(AddTenantUserCommand command, CancellationToken ct)
    {
        logger.LogInformation("Handling AddTenantUserCommand for TenantId: {TenantId}, UserId: {UserId}",
            command.TenantId, command.UserId);

        try
        {
            var tenant = await tenantRepository.GetByIdAsync(new TenantId(command.TenantId), ct)
                ?? throw new InvalidOperationException($"Tenant '{command.TenantId}' not found.");

            // Persist the membership as a new row (INSERT). Using UpdateAsync(tenant) with a
            // NoTracking-loaded tenant marks the new TenantUser as Modified and tries to UPDATE
            // a row that was never inserted -> DbUpdateConcurrencyException.
            var membership = tenant.AddUser(new UserId(command.UserId), command.Role);
            await tenantUserRepository.AddAsync(membership, ct);

            logger.LogInformation("Successfully added user {UserId} to tenant {TenantId}",
                command.UserId, command.TenantId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error adding user {UserId} to tenant {TenantId}",
                command.UserId, command.TenantId);
            throw;
        }
    }
}
