namespace OroIdentityServer.Application.Modules.Hierarchy.Commands;

public record UpdateRelationshipPriorityCommand(Guid RelationshipId, int Priority, Guid? PerformedByUserId) : ICommand;
