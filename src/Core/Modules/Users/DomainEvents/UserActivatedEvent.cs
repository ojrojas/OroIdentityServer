namespace OroIdentityServer.Core.Modules.Users.DomainEvents;

public sealed record UserActivatedEvent(UserId UserId) : DomainEvent;
