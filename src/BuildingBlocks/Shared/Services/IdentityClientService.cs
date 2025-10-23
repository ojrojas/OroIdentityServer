// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.BuildingBlocks.Shared.Services;

public class IdentityClientService(HttpClient httpClient, ILogger<IdentityClientService> logger) : IIdentityClientService
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly ILogger<IdentityClientService> _logger = logger;

    public async Task<RoleInfo?> GetRoleByIdAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation($"Get role by id request to identity server {roleId}");
        var response = await _httpClient.GetAsync($"api/getrolebyid/{roleId}", cancellationToken);
        response.EnsureSuccessStatusCode();
        var stringResponse = await response.Content.ReadAsStringAsync(cancellationToken);
        var roleInfo = JsonSerializer.Deserialize<RoleInfo>(stringResponse);

        return roleInfo;
    }

    public async Task<UserInfo?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation($"Get user by id request to identity server {userId}");

        var response = await _httpClient.GetAsync($"api/getuserbyid/{userId}", cancellationToken);
        response.EnsureSuccessStatusCode();
        var stringResponse = await response.Content.ReadAsStringAsync(cancellationToken);
        var userInfo = JsonSerializer.Deserialize<UserInfo>(stringResponse);

        return userInfo;
    }

    public async Task<IEnumerable<Guid>> GetUserRoleIdsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation($"Get roles ids by id user request to identity server {userId}");

        var response = await _httpClient.GetAsync($"api/getrolesidbyuserid/{userId}", cancellationToken);
        response.EnsureSuccessStatusCode();
        var stringResponse = await response.Content.ReadAsStringAsync(cancellationToken);
        var roleIds = JsonSerializer.Deserialize<IEnumerable<Guid>>(stringResponse);

        return roleIds ?? [];
    }
}