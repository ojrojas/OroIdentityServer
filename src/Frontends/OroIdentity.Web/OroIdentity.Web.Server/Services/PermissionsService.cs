using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using OroIdentity.Web.Client.Interfaces;
using OroIdentity.Web.Client.Models;

namespace OroIdentity.Web.Server.Services;

public class PermissionsService(
    ILogger<PermissionsService> logger,
    HttpClient httpClient) : IPermissionsService
{
    private readonly JsonSerializerOptions options = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    private readonly string ROUTE = "permissions";

    public async Task CreatePermissionAsync(PermissionViewModel permission, CancellationToken cancellationToken)
    {
        await httpClient.PostAsJsonAsync(ROUTE, permission, options, cancellationToken);
    }

    public async Task DeletePermissionAsync(Guid permissionId, CancellationToken cancellationToken)
    {
        await httpClient.DeleteAsync($"{ROUTE}/{permissionId}", cancellationToken);
    }

    public async Task<BaseResponseViewModel<IEnumerable<PermissionViewModel>>> GetAllPermissionsAsync(CancellationToken cancellationToken)
    {
        var response = await httpClient.GetAsync(ROUTE, cancellationToken);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<BaseResponseViewModel<IEnumerable<PermissionViewModel>>>(json, options) ?? new BaseResponseViewModel<IEnumerable<PermissionViewModel>>() { Data = Array.Empty<PermissionViewModel>() };
    }

    public async Task<PermissionViewModel?> GetPermissionByIdAsync(Guid permissionId, CancellationToken cancellationToken)
    {
        return await httpClient.GetFromJsonAsync<PermissionViewModel>($"{ROUTE}/{permissionId}", cancellationToken);
    }

    public async Task UpdatePermissionAsync(PermissionViewModel permission, CancellationToken cancellationToken)
    {
        await httpClient.PutAsJsonAsync(ROUTE, permission, options, cancellationToken);
    }
}
