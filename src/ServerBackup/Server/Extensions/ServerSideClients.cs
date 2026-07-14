// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using System.Net;
using OroIdentityServer.Server.Client.Models;
using OroIdentityServer.Server.Client.Services;

namespace OroIdentityServer.Server.Extensions;

public sealed class UsersServerClient(
    IQueryHandler<GetUsersQuery, GetUsersQueryResponse> getAll)
    : IUsersClient
{
    public async Task<IReadOnlyList<UserDto>> GetAllAsync(CancellationToken ct = default)
        => (await getAll.HandleAsync(new GetUsersQuery(), ct))
           .Data?.Select(MapUser).ToList() ?? [];

    public Task<HttpResponseMessage> CreateAsync(CreateUserModel m, CancellationToken ct = default)
        => ReturnNoContent();

    public Task<HttpResponseMessage> UpdateAsync(Guid id, UpdateUserModel m, CancellationToken ct = default)
        => ReturnNoContent();

    public Task<HttpResponseMessage> DeleteAsync(Guid id, CancellationToken ct = default)
        => ReturnNoContent();

    private static UserDto MapUser(OroIdentityServer.Core.Modules.Users.Aggregates.User u) => new()
    {
        Id = u.Id.Value,
        UserName = u.UserName,
        Email = u.Email,
        Name = u.Name,
        LastName = u.LastName,
        Identification = u.Identification,
    };

    private static Task<HttpResponseMessage> ReturnNoContent()
        => Task.FromResult(new HttpResponseMessage(HttpStatusCode.NoContent));
}

public sealed class RolesServerClient(
    IQueryHandler<GetRolesQuery, GetRolesResponse> getAll)
    : IRolesClient
{
    public async Task<IReadOnlyList<RoleDto>> GetAllAsync(CancellationToken ct = default)
        => (await getAll.HandleAsync(new GetRolesQuery(), ct))
           .Data?.Select(r => new RoleDto(r.Id, r.IsActive, r.Name?.Value ?? "")).ToList() ?? [];

    public Task<HttpResponseMessage> CreateAsync(CreateRoleModel m, CancellationToken ct = default) => ReturnNoContent();
    public Task<HttpResponseMessage> UpdateAsync(Guid id, UpdateRoleModel m, CancellationToken ct = default) => ReturnNoContent();
    public Task<HttpResponseMessage> DeleteAsync(Guid id, CancellationToken ct = default) => ReturnNoContent();

    private static Task<HttpResponseMessage> ReturnNoContent()
        => Task.FromResult(new HttpResponseMessage(HttpStatusCode.NoContent));
}

public sealed class PermissionsServerClient(
    IQueryHandler<GetPermissionsQuery, GetPermissionsQueryResponse> getAll)
    : IPermissionsClient
{
    public async Task<IReadOnlyList<PermissionDto>> GetAllAsync(CancellationToken ct = default)
        => (await getAll.HandleAsync(new GetPermissionsQuery(), ct))
           .Data?.Select(p => new PermissionDto(p.PermissionId, p.Provider, p.Description, p.Action, p.Resource, p.IsSystem)).ToList() ?? [];

    public Task<HttpResponseMessage> CreateAsync(PermissionUpsertModel m, CancellationToken ct = default) => ReturnNoContent();
    public Task<HttpResponseMessage> UpdateAsync(Guid id, PermissionUpsertModel m, CancellationToken ct = default) => ReturnNoContent();
    public Task<HttpResponseMessage> DeleteAsync(Guid id, CancellationToken ct = default) => ReturnNoContent();

    private static Task<HttpResponseMessage> ReturnNoContent()
        => Task.FromResult(new HttpResponseMessage(HttpStatusCode.NoContent));
}

public sealed class TenantsServerClient(
    IQueryHandler<GetTenantsQuery, GetTenantsResponse> getAll,
    IQueryHandler<GetTenantByIdQuery, GetTenantByIdResponse> getById)
    : ITenantsClient
{
    public async Task<IReadOnlyList<TenantDto>> GetAllAsync(CancellationToken ct = default)
        => (await getAll.HandleAsync(new GetTenantsQuery(), ct))
           .Data?.Select(t => new TenantDto(t.Id, t.Name, t.Slug, t.IsActive, t.CreatedAtUtc, t.UserCount)).ToList() ?? [];

    public async Task<TenantDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var resp = await getById.HandleAsync(new GetTenantByIdQuery(id), ct);
        return resp.Data is null ? null : new TenantDto(resp.Data.Id, resp.Data.Name, resp.Data.Slug, resp.Data.IsActive, resp.Data.CreatedAtUtc, resp.Data.UserCount);
    }

    public Task<HttpResponseMessage> CreateAsync(CreateTenantModel m, CancellationToken ct = default) => ReturnNoContent();
    public Task<HttpResponseMessage> UpdateAsync(Guid id, UpdateTenantModel m, CancellationToken ct = default) => ReturnNoContent();
    public Task<HttpResponseMessage> ActivateAsync(Guid id, CancellationToken ct = default) => ReturnNoContent();
    public Task<HttpResponseMessage> SuspendAsync(Guid id, CancellationToken ct = default) => ReturnNoContent();

    private static Task<HttpResponseMessage> ReturnNoContent()
        => Task.FromResult(new HttpResponseMessage(HttpStatusCode.NoContent));
}

public sealed class IdentificationTypesServerClient(
    IQueryHandler<GetIdentificationTypesQuery, GetIdentificationTypesResponse> getAll)
    : IIdentificationTypesClient
{
    public async Task<IReadOnlyList<IdentificationTypeDto>> GetAllAsync(CancellationToken ct = default)
        => (await getAll.HandleAsync(new GetIdentificationTypesQuery(), ct))
           .Data?.Select(i => new IdentificationTypeDto(i.Id, i.Name, i.IsActive, i.CreatedAtUtc)).ToList() ?? [];

    public Task<HttpResponseMessage> CreateAsync(IdentificationTypeUpsertModel m, CancellationToken ct = default) => ReturnNoContent();
    public Task<HttpResponseMessage> UpdateAsync(Guid id, IdentificationTypeUpsertModel m, CancellationToken ct = default) => ReturnNoContent();
    public Task<HttpResponseMessage> DeleteAsync(Guid id, CancellationToken ct = default) => ReturnNoContent();

    private static Task<HttpResponseMessage> ReturnNoContent()
        => Task.FromResult(new HttpResponseMessage(HttpStatusCode.NoContent));
}

public sealed class UserSessionsServerClient(
    IQueryHandler<GetUserSessionsByUserQuery, IEnumerable<OroIdentityServer.Core.Modules.UserSessions.Aggregates.UserSession>> getByUser)
    : IUserSessionsClient
{
    public async Task<IReadOnlyList<UserSessionDto>> GetByUserAsync(Guid userId, CancellationToken ct = default)
        => (await getByUser.HandleAsync(new GetUserSessionsByUserQuery(userId), ct))
           .Select(s => new UserSessionDto
           {
               Id = s.Id.Value,
               UserId = s.UserId!.Value,
               Device = s.Device,
               ExpiresAt = s.ExpiresAt,
               IpAddress = s.IpAddress,
               UserAgent = s.UserAgent,
               Location = s.Location,
               IsActive = s.ExpiresAt > DateTime.UtcNow
           }).ToList();

    public Task<HttpResponseMessage> DeactivateAsync(Guid sessionId, CancellationToken ct = default) => ReturnNoContent();

    private static Task<HttpResponseMessage> ReturnNoContent()
        => Task.FromResult(new HttpResponseMessage(HttpStatusCode.NoContent));
}

public sealed class ApplicationsServerClient(
    IQueryHandler<GetApplicationsQuery, IEnumerable<global::OpenIddict.Abstractions.OpenIddictApplicationDescriptor>> getAll)
    : IApplicationsClient
{
    public async Task<IReadOnlyList<ApplicationDto>> GetAllAsync(CancellationToken ct = default)
        => (await getAll.HandleAsync(new GetApplicationsQuery(), ct))
           .Select(a => new ApplicationDto
           {
               ClientId = a.ClientId,
               DisplayName = a.DisplayName,
               ClientType = a.ClientType,
               ApplicationType = a.ApplicationType,
               ConsentType = a.ConsentType,
               Permissions = a.Permissions.ToList(),
               Requirements = a.Requirements.ToList(),
               RedirectUris = a.RedirectUris.Select(u => u.AbsoluteUri).ToList(),
               PostLogoutRedirectUris = a.PostLogoutRedirectUris.Select(u => u.AbsoluteUri).ToList()
           }).ToList();

    public Task<HttpResponseMessage> CreateAsync(ApplicationDto m, CancellationToken ct = default) => ReturnNoContent();
    public Task<HttpResponseMessage> UpdateAsync(string clientId, ApplicationDto m, CancellationToken ct = default) => ReturnNoContent();
    public Task<HttpResponseMessage> DeleteAsync(string clientId, CancellationToken ct = default) => ReturnNoContent();

    private static Task<HttpResponseMessage> ReturnNoContent()
        => Task.FromResult(new HttpResponseMessage(HttpStatusCode.NoContent));
}

public sealed class ScopesServerClient(
    IQueryHandler<GetScopesQuery, IEnumerable<global::OpenIddict.Abstractions.OpenIddictScopeDescriptor>> getAll)
    : IScopesClient
{
    public async Task<IReadOnlyList<ScopeDto>> GetAllAsync(CancellationToken ct = default)
        => (await getAll.HandleAsync(new GetScopesQuery(), ct))
           .Select(s => new ScopeDto { Name = s.Name, Resources = s.Resources.ToList() }).ToList();

    public Task<HttpResponseMessage> CreateAsync(ScopeDto m, CancellationToken ct = default) => ReturnNoContent();
    public Task<HttpResponseMessage> UpdateAsync(string name, ScopeDto m, CancellationToken ct = default) => ReturnNoContent();
    public Task<HttpResponseMessage> DeleteAsync(string name, CancellationToken ct = default) => ReturnNoContent();

    private static Task<HttpResponseMessage> ReturnNoContent()
        => Task.FromResult(new HttpResponseMessage(HttpStatusCode.NoContent));
}
