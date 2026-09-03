using System.Net;
using System.Security.Cryptography;
using BuildingBlocks.CQRS.Abstractions;
using IdentityServer.Client.Interfaces;
using IdentityServer.Client.Models;
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
    public async Task<PagedResponse<OpenIddictApplicationModel>?> GetApplicationsAsync(PagedRequest? request = null, CancellationToken ct = default)
    {
        var req = request ?? new PagedRequest();
        var result = await queryDispatcher.SendAsync(new GetApplicationsQuery(req.SearchTerm, req.PageNumber, req.PageSize), ct);
        return new PagedResponse<OpenIddictApplicationModel>
        {
            Items = result.Data.Select(MapApplication).ToList(),
            TotalCount = result.TotalCount,
            PageNumber = result.PageNumber,
            PageSize = result.PageSize
        };
    }

    public async Task<OpenIddictApplicationModel?> GetApplicationByClientIdAsync(string clientId, CancellationToken ct = default)
    {
        var application = await queryDispatcher.SendAsync(new GetApplicationByClientIdQuery(clientId), ct);
        return application is null ? null : MapApplication(application);
    }

    public async Task<HttpResponseMessage> CreateApplicationAsync(OpenIddictApplicationModel application, CancellationToken ct = default)
    {
        var descriptor = MapDescriptor(application);

        if (descriptor.ClientType == "confidential" && string.IsNullOrWhiteSpace(descriptor.ClientSecret))
        {
            descriptor.ClientSecret = GenerateSecret();
        }

        var result = await commandDispatcher.SendAsync(new CreateApplicationCommand(descriptor), ct);
        return HttpResponseMessageFactory.FromResult(result, HttpStatusCode.Created);
    }

    public async Task<HttpResponseMessage> UpdateApplicationAsync(string clientId, OpenIddictApplicationModel application, CancellationToken ct = default)
    {
        var descriptor = MapDescriptor(application);
        descriptor.ClientId = clientId;

        if (descriptor.ClientType == "confidential" && string.IsNullOrWhiteSpace(descriptor.ClientSecret))
        {
            var existing = await queryDispatcher.SendAsync(new GetApplicationByClientIdQuery(clientId), ct);
            if (existing is not null)
                descriptor.ClientSecret = existing.ClientSecret;
        }

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
        MaskSecret(descriptor.ClientSecret, descriptor.ClientType),
        descriptor.DisplayName,
        descriptor.ClientType,
        descriptor.ApplicationType,
        descriptor.ConsentType,
        [.. descriptor.Permissions],
        [.. descriptor.Requirements],
        [.. descriptor.RedirectUris.Select(u => u.ToString())],
        [.. descriptor.PostLogoutRedirectUris.Select(u => u.ToString())]);

    private static string? MaskSecret(string? secret, string? clientType)
    {
        if (string.IsNullOrWhiteSpace(secret) || clientType != "confidential")
            return secret;

        if (secret.Length <= 6)
            return "sk-****";

        return $"sk-****{secret[^3..]}";
    }

    private static string GenerateSecret()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes);
    }

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
