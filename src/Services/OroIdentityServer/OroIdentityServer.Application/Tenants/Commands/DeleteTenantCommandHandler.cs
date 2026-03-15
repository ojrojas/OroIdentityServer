// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Services.OroIdentityServer.Application.Commands;

public class DeleteTenantCommandHandler(
    ILogger<DeleteTenantCommandHandler> logger,
    ITenantRepository tenantRepository
    ) : ICommandHandler<DeleteTenantCommand>
{
    private readonly ILogger<DeleteTenantCommandHandler> _logger = logger;
    private readonly ITenantRepository _tenantRepository = tenantRepository;

    public async Task HandleAsync(DeleteTenantCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling DeleteTenantCommand for Id: {Id}", command.Id);
        await _tenantRepository.DeleteTenantAsync(command.Id, cancellationToken);
        _logger.LogInformation("Tenant deleted: {Id}", command.Id);
    }
}
