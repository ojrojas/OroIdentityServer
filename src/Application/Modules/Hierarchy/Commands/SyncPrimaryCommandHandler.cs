using BuildingBlocks.CQRS.Abstractions;
using BuildingBlocks.Kernel.Results;
using OroIdentityServer.Core.Modules.Hierarchy.Services;
using OroIdentityServer.Core.Shared;

namespace OroIdentityServer.Application.Modules.Hierarchy.Commands;

public class SyncPrimaryCommandHandler(
    IHierarchyService hierarchyService,
    ILogger<SyncPrimaryCommandHandler> logger) : ICommandHandler<SyncPrimaryCommand>
{
    public async Task<Result> HandleAsync(SyncPrimaryCommand command, CancellationToken ct)
    {
        try
        {
            await hierarchyService.SyncPrimaryReportsToAsync(new TenantId(command.TenantId), new UserId(command.UserId), ct);
            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Sync primary failed");
            return Result.Failure(new Error("SyncPrimaryFailed", ex.Message, ErrorType.Failure));
        }
    }
}
