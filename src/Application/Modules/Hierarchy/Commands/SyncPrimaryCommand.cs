namespace OroIdentityServer.Application.Modules.Hierarchy.Commands;

public record SyncPrimaryCommand(Guid TenantId, Guid UserId) : ICommand;
