// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
using BuildingBlocks.CQRS.Abstractions;
using BuildingBlocks.Kernel.Results;
using OroIdentityServer.Core.Modules.Hierarchy.Enums;
using OroIdentityServer.Core.Modules.Hierarchy.Services;
using OroIdentityServer.Core.Modules.Hierarchy.ValueObjects;
using OroIdentityServer.Core.Shared;

namespace OroIdentityServer.Application.Modules.Hierarchy.Commands;

public class CreateRelationshipCommandHandler(
    IHierarchyService hierarchyService,
    ILogger<CreateRelationshipCommandHandler> logger) : ICommandHandler<CreateRelationshipCommand>
{
    public async Task<Result> HandleAsync(CreateRelationshipCommand command, CancellationToken ct)
    {
        try
        {
            if (!Enum.TryParse<RelationshipType>(command.Type, true, out var type))
                return Result.Failure(Error.Validation("InvalidType", $"Invalid relationship type {command.Type}"));

            await hierarchyService.CreateRelationshipAsync(
                new TenantId(command.TenantId),
                new UserId(command.UserId),
                new UserId(command.ReportsToUserId),
                type,
                command.Priority,
                command.PerformedByUserId.HasValue ? new UserId(command.PerformedByUserId.Value) : null,
                ct);

            return Result.Success();
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("duplicate", StringComparison.OrdinalIgnoreCase))
        {
            logger.LogWarning(ex, "Duplicate relationship");
            return Result.Failure(Error.Validation("DuplicateRelationship", ex.Message));
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("cycle", StringComparison.OrdinalIgnoreCase) || ex.Message.Contains("own superior", StringComparison.OrdinalIgnoreCase))
        {
            logger.LogWarning(ex, "Cycle validation");
            return Result.Failure(Error.Validation("CycleDetected", ex.Message));
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("maximum", StringComparison.OrdinalIgnoreCase))
        {
            return Result.Failure(Error.Validation("MaxSuperiorsExceeded", ex.Message));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Create relationship failed");
            return Result.Failure(new Error("CreateRelationshipFailed", ex.Message, ErrorType.Failure));
        }
    }
}
