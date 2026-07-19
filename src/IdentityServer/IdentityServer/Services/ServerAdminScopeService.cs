using System.Net;
using BuildingBlocks.CQRS.Abstractions;
using IdentityServer.Client.Interfaces;
using IdentityServer.Client.Models.OpenIddict;
using OpenIddict.Abstractions;
using OroIdentityServer.Application.Modules.Openddict.Commands;
using OroIdentityServer.Application.Modules.Openddict.Queries;

namespace IdentityServer.Services;

public class ServerAdminScopeService(IQueryDispatcher queryDispatcher, ICommandDispatcher commandDispatcher) : IAdminScopeService
{
    public async Task<IEnumerable<OpenIddictScopeModel>?> GetScopesAsync(CancellationToken ct = default)
    {
        var scopes = await queryDispatcher.SendAsync(new GetScopesQuery(), ct);
        return scopes.Select(MapScope).ToList();
    }

    public async Task<HttpResponseMessage> CreateScopeAsync(CreateOpenIddictScopeRequest request, CancellationToken ct = default)
    {
        var result = await commandDispatcher.SendAsync(new CreateScopeCommand(request.Name, request.Resources), ct);
        return HttpResponseMessageFactory.FromResult(result, HttpStatusCode.Created);
    }

    public async Task<HttpResponseMessage> UpdateScopeAsync(string name, UpdateOpenIddictScopeRequest request, CancellationToken ct = default)
    {
        var result = await commandDispatcher.SendAsync(new UpdateScopeCommand(name, request.Resources), ct);
        return HttpResponseMessageFactory.FromResult(result, HttpStatusCode.NoContent);
    }

    public async Task<HttpResponseMessage> DeleteScopeAsync(string name, CancellationToken ct = default)
    {
        var result = await commandDispatcher.SendAsync(new DeleteScopeCommand(name), ct);
        return HttpResponseMessageFactory.FromResult(result, HttpStatusCode.NoContent);
    }

    private static OpenIddictScopeModel MapScope(OpenIddictScopeDescriptor descriptor) => new(
        descriptor.Name, descriptor.DisplayName, descriptor.Description, [.. descriptor.Resources]);
}
