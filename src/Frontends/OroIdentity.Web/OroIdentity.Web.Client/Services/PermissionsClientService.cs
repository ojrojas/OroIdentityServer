using System.Net.Http.Json;
using OroIdentity.Web.Client.Interfaces;
using OroIdentity.Web.Client.Models;

namespace OroIdentity.Web.Client.Services;

internal sealed class PermissionsClientService(
    ILogger<PermissionsClientService> logger,
    HttpClient httpClient) : IPermissionsService
{
    private readonly string ROUTE = "api/v1/permissions";

    public async Task CreatePermissionAsync(PermissionViewModel permission, CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating permission: {Name}", permission.Name);
        await httpClient.PostAsJsonAsync(ROUTE, permission, cancellationToken);
    }

    public async Task DeletePermissionAsync(Guid permissionId, CancellationToken cancellationToken)
    {
        logger.LogInformation("Deleting permission: {Id}", permissionId);
        await httpClient.DeleteAsync($"{ROUTE}/{permissionId}", cancellationToken);
    }

    public async Task<BaseResponseViewModel<IEnumerable<PermissionViewModel>>> GetAllPermissionsAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving all permissions");
        return await httpClient.GetFromJsonAsync<BaseResponseViewModel<IEnumerable<PermissionViewModel>>>(ROUTE, cancellationToken) ?? new BaseResponseViewModel<IEnumerable<PermissionViewModel>>() { Data = Array.Empty<PermissionViewModel>() };
    }

    public async Task<PermissionViewModel?> GetPermissionByIdAsync(Guid permissionId, CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving permission {Id}", permissionId);
        return await httpClient.GetFromJsonAsync<PermissionViewModel>($"{ROUTE}/{permissionId}", cancellationToken);
    }

    public async Task UpdatePermissionAsync(PermissionViewModel permission, CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating permission {Id}", permission.PermissionId);
        await httpClient.PutAsJsonAsync(ROUTE, permission, cancellationToken);
    }
}
