using System.Net;
using BuildingBlocks.CQRS.Abstractions;
using IdentityServer.Client.Interfaces;
using IdentityServer.Client.Models.OpenIddict;
using OpenIddict.Abstractions;
using OroIdentityServer.Application.Modules.Openddict.Commands;
using OroIdentityServer.Application.Modules.Openddict.Queries;
using OroIdentityServer.Application.Shared;

namespace IdentityServer.Services;

public class ServerAdminApplicationService(
    IQueryDispatcher queryDispatcher, 
    ICommandDispatcher commandDispatcher) : IAdminApplicationService
{
    public async Task<IEnumerable<OpenIddictApplicationModel>?> GetApplicationsAsync(CancellationToken ct = default)
    {
        var applications = await queryDispatcher.SendAsync(new GetApplicationsQuery(), ct);
        return [.. applications.Select(MapApplication)];
    }

    public async Task<OpenIddictApplicationModel?> GetApplicationByClientIdAsync(string clientId, CancellationToken ct = default)
    {
        var application = await queryDispatcher.SendAsync(new GetApplicationByClientIdQuery(clientId), ct);
        return application is null ? null : MapApplication(application);
    }

    public async Task<HttpResponseMessage> CreateApplicationAsync(OpenIddictApplicationModel application, CancellationToken ct = default)
    {
        var result = await commandDispatcher.SendAsync(new CreateApplicationCommand(MapDescriptor(application)), ct);
        return HttpResponseMessageFactory.FromResult(result, HttpStatusCode.Created);
    }

    public async Task<HttpResponseMessage> UpdateApplicationAsync(string clientId, OpenIddictApplicationModel application, CancellationToken ct = default)
    {
        var descriptor = MapDescriptor(application);
        descriptor.ClientId = clientId;
        var result = await commandDispatcher.SendAsync(new UpdateApplicationCommand(descriptor), ct);
        return HttpResponseMessageFactory.FromResult(result, HttpStatusCode.NoContent);
    }

    public async Task<HttpResponseMessage> DeleteApplicationAsync(string clientId, CancellationToken ct = default)
    {
        var result = await commandDispatcher.SendAsync(new DeleteApplicationCommand(clientId), ct);
        return HttpResponseMessageFactory.FromResult(result, HttpStatusCode.NoContent);
    }

    private static OpenIddictApplicationModel MapApplication(OpenIddictApplicationDescriptor descriptor) => new(
        descriptor.ClientId,
        descriptor.ClientSecret,
        descriptor.DisplayName,
        descriptor.ClientType,
        descriptor.ApplicationType,
        descriptor.ConsentType,
        [.. descriptor.Permissions],
        [.. descriptor.Requirements],
        [.. descriptor.RedirectUris.Select(u => u.ToString())],
        [.. descriptor.PostLogoutRedirectUris.Select(u => u.ToString())]);

    private static ApplicationDescriptor MapDescriptor(OpenIddictApplicationModel model)
    {
        var descriptor = new ApplicationDescriptor
        {
            ClientId = model.ClientId,
            DisplayName = model.DisplayName,
            ClientSecret = model.ClientSecret,
            ClientType = model.ClientType,
            ApplicationType = model.ApplicationType,
            ConsentType = model.ConsentType
        };

        foreach (var permission in model.Permissions ?? []) descriptor.Permissions.Add(permission);
        foreach (var requirement in model.Requirements ?? []) descriptor.Requirements.Add(requirement);
        foreach (var uri in model.RedirectUris ?? []) descriptor.RedirectUris.Add(new Uri(uri));
        foreach (var uri in model.PostLogoutRedirectUris ?? []) descriptor.PostLogoutRedirectUris.Add(new Uri(uri));

        return descriptor;
    }
}
