// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using System.Net.Http.Json;
using OroIdentityServer.Server.Client.Models;

namespace OroIdentityServer.Server.Client.Services;

// ── Interfaces ──────────────────────────────────────────

public interface IUsersClient
{
    Task<IReadOnlyList<UserDto>> GetAllAsync(CancellationToken ct = default);
    Task<HttpResponseMessage> CreateAsync(CreateUserModel m, CancellationToken ct = default);
    Task<HttpResponseMessage> UpdateAsync(Guid id, UpdateUserModel m, CancellationToken ct = default);
    Task<HttpResponseMessage> DeleteAsync(Guid id, CancellationToken ct = default);
}

public interface IRolesClient
{
    Task<IReadOnlyList<RoleDto>> GetAllAsync(CancellationToken ct = default);
    Task<HttpResponseMessage> CreateAsync(CreateRoleModel m, CancellationToken ct = default);
    Task<HttpResponseMessage> UpdateAsync(Guid id, UpdateRoleModel m, CancellationToken ct = default);
    Task<HttpResponseMessage> DeleteAsync(Guid id, CancellationToken ct = default);
}

public interface IPermissionsClient
{
    Task<IReadOnlyList<PermissionDto>> GetAllAsync(CancellationToken ct = default);
    Task<HttpResponseMessage> CreateAsync(PermissionUpsertModel m, CancellationToken ct = default);
    Task<HttpResponseMessage> UpdateAsync(Guid id, PermissionUpsertModel m, CancellationToken ct = default);
    Task<HttpResponseMessage> DeleteAsync(Guid id, CancellationToken ct = default);
}

public interface ITenantsClient
{
    Task<IReadOnlyList<TenantDto>> GetAllAsync(CancellationToken ct = default);
    Task<TenantDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<HttpResponseMessage> CreateAsync(CreateTenantModel m, CancellationToken ct = default);
    Task<HttpResponseMessage> UpdateAsync(Guid id, UpdateTenantModel m, CancellationToken ct = default);
    Task<HttpResponseMessage> ActivateAsync(Guid id, CancellationToken ct = default);
    Task<HttpResponseMessage> SuspendAsync(Guid id, CancellationToken ct = default);
}

public interface IIdentificationTypesClient
{
    Task<IReadOnlyList<IdentificationTypeDto>> GetAllAsync(CancellationToken ct = default);
    Task<HttpResponseMessage> CreateAsync(IdentificationTypeUpsertModel m, CancellationToken ct = default);
    Task<HttpResponseMessage> UpdateAsync(Guid id, IdentificationTypeUpsertModel m, CancellationToken ct = default);
    Task<HttpResponseMessage> DeleteAsync(Guid id, CancellationToken ct = default);
}

public interface IUserSessionsClient
{
    Task<IReadOnlyList<UserSessionDto>> GetByUserAsync(Guid userId, CancellationToken ct = default);
    Task<HttpResponseMessage> DeactivateAsync(Guid sessionId, CancellationToken ct = default);
}

public interface IApplicationsClient
{
    Task<IReadOnlyList<ApplicationDto>> GetAllAsync(CancellationToken ct = default);
    Task<HttpResponseMessage> CreateAsync(ApplicationDto m, CancellationToken ct = default);
    Task<HttpResponseMessage> UpdateAsync(string clientId, ApplicationDto m, CancellationToken ct = default);
    Task<HttpResponseMessage> DeleteAsync(string clientId, CancellationToken ct = default);
}

public interface IScopesClient
{
    Task<IReadOnlyList<ScopeDto>> GetAllAsync(CancellationToken ct = default);
    Task<HttpResponseMessage> CreateAsync(ScopeDto m, CancellationToken ct = default);
    Task<HttpResponseMessage> UpdateAsync(string name, ScopeDto m, CancellationToken ct = default);
    Task<HttpResponseMessage> DeleteAsync(string name, CancellationToken ct = default);
}

// ── HTTP (WASM) implementations ─────────────────────────

public sealed class UsersClient(HttpClient http) : IUsersClient
{
    public async Task<IReadOnlyList<UserDto>> GetAllAsync(CancellationToken ct = default)
    {
        var env = await http.GetFromJsonAsync<ApiEnvelope<List<UserDto>>>("api/users", ct);
        return env?.Data ?? new();
    }
    public Task<HttpResponseMessage> CreateAsync(CreateUserModel m, CancellationToken ct = default) => http.PostAsJsonAsync("api/users", m, ct);
    public Task<HttpResponseMessage> UpdateAsync(Guid id, UpdateUserModel m, CancellationToken ct = default) => http.PutAsJsonAsync($"api/users/{id}", m, ct);
    public Task<HttpResponseMessage> DeleteAsync(Guid id, CancellationToken ct = default) => http.DeleteAsync($"api/users/{id}", ct);
}

public sealed class RolesClient(HttpClient http) : IRolesClient
{
    public async Task<IReadOnlyList<RoleDto>> GetAllAsync(CancellationToken ct = default)
    {
        var env = await http.GetFromJsonAsync<ApiEnvelope<List<RoleDto>>>("api/roles", ct);
        return env?.Data ?? new();
    }
    public Task<HttpResponseMessage> CreateAsync(CreateRoleModel m, CancellationToken ct = default) => http.PostAsJsonAsync("api/roles", m, ct);
    public Task<HttpResponseMessage> UpdateAsync(Guid id, UpdateRoleModel m, CancellationToken ct = default) => http.PutAsJsonAsync($"api/roles/{id}", m, ct);
    public Task<HttpResponseMessage> DeleteAsync(Guid id, CancellationToken ct = default) => http.DeleteAsync($"api/roles/{id}", ct);
}

public sealed class PermissionsClient(HttpClient http) : IPermissionsClient
{
    public async Task<IReadOnlyList<PermissionDto>> GetAllAsync(CancellationToken ct = default)
    {
        var env = await http.GetFromJsonAsync<ApiEnvelope<List<PermissionDto>>>("api/permissions", ct);
        return env?.Data ?? new();
    }
    public Task<HttpResponseMessage> CreateAsync(PermissionUpsertModel m, CancellationToken ct = default) => http.PostAsJsonAsync("api/permissions", m, ct);
    public Task<HttpResponseMessage> UpdateAsync(Guid id, PermissionUpsertModel m, CancellationToken ct = default) => http.PutAsJsonAsync($"api/permissions/{id}", m, ct);
    public Task<HttpResponseMessage> DeleteAsync(Guid id, CancellationToken ct = default) => http.DeleteAsync($"api/permissions/{id}", ct);
}

public sealed class TenantsClient(HttpClient http) : ITenantsClient
{
    public async Task<IReadOnlyList<TenantDto>> GetAllAsync(CancellationToken ct = default)
    {
        var env = await http.GetFromJsonAsync<ApiEnvelope<List<TenantDto>>>("api/tenants", ct);
        return env?.Data ?? [];
    }
    public async Task<TenantDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var env = await http.GetFromJsonAsync<ApiEnvelope<TenantDto>>($"api/tenants/{id}", ct);
        return env?.Data;
    }
    public Task<HttpResponseMessage> CreateAsync(CreateTenantModel m, CancellationToken ct = default) => http.PostAsJsonAsync("api/tenants", m, ct);
    public Task<HttpResponseMessage> UpdateAsync(Guid id, UpdateTenantModel m, CancellationToken ct = default) => http.PutAsJsonAsync($"api/tenants/{id}", m, ct);
    public Task<HttpResponseMessage> ActivateAsync(Guid id, CancellationToken ct = default) => http.PostAsync($"api/tenants/{id}/activate", null, ct);
    public Task<HttpResponseMessage> SuspendAsync(Guid id, CancellationToken ct = default) => http.PostAsync($"api/tenants/{id}/suspend", null, ct);
}

public sealed class IdentificationTypesClient(HttpClient http) : IIdentificationTypesClient
{
    public async Task<IReadOnlyList<IdentificationTypeDto>> GetAllAsync(CancellationToken ct = default)
    {
        var env = await http.GetFromJsonAsync<ApiEnvelope<List<IdentificationTypeDto>>>("api/identification-types", ct);
        return env?.Data ?? new();
    }
    public Task<HttpResponseMessage> CreateAsync(IdentificationTypeUpsertModel m, CancellationToken ct = default) => http.PostAsJsonAsync("api/identification-types", m, ct);
    public Task<HttpResponseMessage> UpdateAsync(Guid id, IdentificationTypeUpsertModel m, CancellationToken ct = default) => http.PutAsJsonAsync($"api/identification-types/{id}", m, ct);
    public Task<HttpResponseMessage> DeleteAsync(Guid id, CancellationToken ct = default) => http.DeleteAsync($"api/identification-types/{id}", ct);
}

public sealed class UserSessionsClient(HttpClient http) : IUserSessionsClient
{
    public async Task<IReadOnlyList<UserSessionDto>> GetByUserAsync(Guid userId, CancellationToken ct = default)
        => await http.GetFromJsonAsync<List<UserSessionDto>>($"api/user-sessions/by-user/{userId}", ct) ?? new();
    public Task<HttpResponseMessage> DeactivateAsync(Guid sessionId, CancellationToken ct = default) => http.PostAsync($"api/user-sessions/{sessionId}/deactivate", null, ct);
}

public sealed class ApplicationsClient(HttpClient http) : IApplicationsClient
{
    public async Task<IReadOnlyList<ApplicationDto>> GetAllAsync(CancellationToken ct = default)
        => await http.GetFromJsonAsync<List<ApplicationDto>>("api/applications", ct) ?? new();
    public Task<HttpResponseMessage> CreateAsync(ApplicationDto m, CancellationToken ct = default) => http.PostAsJsonAsync("api/applications", m, ct);
    public Task<HttpResponseMessage> UpdateAsync(string clientId, ApplicationDto m, CancellationToken ct = default) => http.PutAsJsonAsync($"api/applications/{clientId}", m, ct);
    public Task<HttpResponseMessage> DeleteAsync(string clientId, CancellationToken ct = default) => http.DeleteAsync($"api/applications/{clientId}", ct);
}

public sealed class ScopesClient(HttpClient http) : IScopesClient
{
    public async Task<IReadOnlyList<ScopeDto>> GetAllAsync(CancellationToken ct = default)
        => await http.GetFromJsonAsync<List<ScopeDto>>("api/scopes", ct) ?? new();
    public Task<HttpResponseMessage> CreateAsync(ScopeDto m, CancellationToken ct = default) => http.PostAsJsonAsync("api/scopes", new { name = m.Name, resources = m.Resources }, ct);
    public Task<HttpResponseMessage> UpdateAsync(string name, ScopeDto m, CancellationToken ct = default) => http.PutAsJsonAsync($"api/scopes/{name}", new { name, resources = m.Resources }, ct);
    public Task<HttpResponseMessage> DeleteAsync(string name, CancellationToken ct = default) => http.DeleteAsync($"api/scopes/{name}", ct);
}
