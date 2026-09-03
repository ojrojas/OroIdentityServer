namespace OroIdentityServer.Core.Modules.Roles.DomainEvents;

public sealed record RoleActivatedEvent(RoleId RoleId) : DomainEvent;
