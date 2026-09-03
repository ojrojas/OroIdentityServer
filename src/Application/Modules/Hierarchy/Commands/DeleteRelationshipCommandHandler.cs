using OroIdentityServer.Core.Modules.Hierarchy.Services;
using OroIdentityServer.Core.Modules.Hierarchy.ValueObjects;

namespace OroIdentityServer.Application.Modules.Hierarchy.Commands;

public class DeleteRelationshipCommandHandler(
    IHierarchyService hierarchyService,
    ILogger<DeleteRelationshipCommandHandler> logger) : ICommandHandler<DeleteRelationshipCommand>
{
    public async Task<Result> HandleAsync(DeleteRelationshipCommand command, CancellationToken ct)
    {
        try
        {
            await hierarchyService.DeleteRelationshipAsync(
                new UserReportingRelationshipId(command.RelationshipId),
                command.PerformedByUserId.HasValue ? new OroIdentityServer.Core.Shared.UserId(command.PerformedByUserId.Value) : null,
                command.Reason,
                ct);
            return Result.Success();
        }
        catch (KeyNotFoundException ex)
        {
            return Result.Failure(Error.NotFound("RelationshipNotFound", ex.Message));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Delete relationship failed");
            return Result.Failure(new Error("DeleteRelationshipFailed", ex.Message, ErrorType.Failure));
        }
    }
}
