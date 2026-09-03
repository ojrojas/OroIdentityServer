namespace OroIdentityServer.Core.Modules.Users.DomainEvents;

public sealed record UserDeactivatedEvent(UserId UserId) : DomainEvent;
