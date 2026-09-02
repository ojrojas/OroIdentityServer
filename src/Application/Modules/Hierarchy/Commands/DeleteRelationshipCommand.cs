using BuildingBlocks.CQRS.Abstractions;

namespace OroIdentityServer.Application.Modules.Hierarchy.Commands;

public record DeleteRelationshipCommand(Guid RelationshipId, Guid? PerformedByUserId, string? Reason) : ICommand;
