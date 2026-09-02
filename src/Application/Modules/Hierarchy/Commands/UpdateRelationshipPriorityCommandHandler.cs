using BuildingBlocks.CQRS.Abstractions;
using BuildingBlocks.Kernel.Results;
using OroIdentityServer.Core.Modules.Hierarchy.Services;
using OroIdentityServer.Core.Modules.Hierarchy.ValueObjects;

namespace OroIdentityServer.Application.Modules.Hierarchy.Commands;

public class UpdateRelationshipPriorityCommandHandler(
    IHierarchyService hierarchyService,
    ILogger<UpdateRelationshipPriorityCommandHandler> logger) : ICommandHandler<UpdateRelationshipPriorityCommand>
{
    public async Task<Result> HandleAsync(UpdateRelationshipPriorityCommand command, CancellationToken ct)
    {
        try
        {
            await hierarchyService.UpdateRelationshipPriorityAsync(
                new UserReportingRelationshipId(command.RelationshipId),
                command.Priority,
                command.PerformedByUserId.HasValue ? new OroIdentityServer.Core.Shared.UserId(command.PerformedByUserId.Value) : null,
                ct);
            return Result.Success();
        }
        catch (KeyNotFoundException ex)
        {
            return Result.Failure(Error.NotFound("RelationshipNotFound", ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(Error.Validation("InvalidPriority", ex.Message));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Update priority failed");
            return Result.Failure(new Error("UpdatePriorityFailed", ex.Message, ErrorType.Failure));
        }
    }
}
