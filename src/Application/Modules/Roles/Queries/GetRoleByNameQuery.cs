namespace OroIdentityServer.Application.Modules.Roles.Queries;

public record GetRoleByNameQuery(string Name) : IQuery<GetRoleByNameResponse>
{
     public Guid CorrelationId() => Guid.NewGuid();
}
