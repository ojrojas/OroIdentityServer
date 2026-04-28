// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Application.Modules.Tenants.Commands;

public sealed class CreateTenantCommandHandler(
    ILogger<CreateTenantCommandHandler> logger,
    ITenantRepository tenantRepository,
    IEventBus eventBus)
    : ICommandHandler<CreateTenantCommand>
{
    public async Task HandleAsync(CreateTenantCommand command, CancellationToken ct)
    {
        logger.LogInformation("Handling CreateTenantCommand for Slug: {Slug}", command.Slug);

        try
        {
            var slug = new TenantSlug(command.Slug);

            if (await tenantRepository.SlugExistsAsync(slug, ct))
                throw new InvalidOperationException($"A tenant with slug '{command.Slug}' already exists.");

            var tenant = Tenant.Create(command.Name);

            await tenantRepository.AddAsync(tenant, ct);

            await eventBus.PublishAsync(
                new TenantProvisionedIntegrationEvent(tenant.Id.Value, command.Slug), ct);

            logger.LogInformation("Successfully created tenant with Slug: {Slug}", command.Slug);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating tenant with Slug: {Slug}", command.Slug);
            throw;
        }
    }
}
