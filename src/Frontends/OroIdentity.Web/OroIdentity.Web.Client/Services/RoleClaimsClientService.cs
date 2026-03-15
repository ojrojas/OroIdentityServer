using OroIdentity.Web.Client.Interfaces;
using OroIdentity.Web.Client.Models;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Net.Http.Json;

namespace OroIdentity.Web.Client.Services;

public class RoleClaimsClientService(
    ILogger<RoleClaimsClientService> logger,
    HttpClient httpClient) : IRoleClaimsService
{
    private readonly JsonSerializerOptions options = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    private readonly string ROUTE = "api/v1/roleclaims";

    public async Task<RoleClaimViewModel?> GetRoleClaimByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await httpClient.GetFromJsonAsync<RoleClaimViewModel>($"{ROUTE}/get/{id}", cancellationToken);
    }

    public async Task<BaseResponseViewModel<IEnumerable<RoleClaimViewModel>>> GetRoleClaimsByRoleIdAsync(Guid roleId, CancellationToken cancellationToken)
    {
        var response = await httpClient.GetFromJsonAsync<BaseResponseViewModel<IEnumerable<RoleClaimViewModel>>>($"{ROUTE}/getbyrole/{roleId}", cancellationToken);
        return response ?? new BaseResponseViewModel<IEnumerable<RoleClaimViewModel>> { Data = Array.Empty<RoleClaimViewModel>() };
    }

    public async Task AssociateClaimToRoleAsync(Guid roleId, string claimType, string claimValue, CancellationToken cancellationToken)
    {
        // Web.Server expects a payload shaped like RoleClaimViewModel where Id.Value is the RoleId
        var payload = new { Id = new { Value = roleId }, ClaimType = claimType, ClaimValue = claimValue };
        await httpClient.PostAsJsonAsync($"{ROUTE}/associate", payload, cancellationToken);
    }

    public async Task UpdateRoleClaimAsync(RoleClaimViewModel claim, CancellationToken cancellationToken)
    {
        await httpClient.PutAsJsonAsync($"{ROUTE}/update", claim, cancellationToken);
    }

    public async Task DeleteRoleClaimAsync(Guid id, CancellationToken cancellationToken)
    {
        await httpClient.DeleteAsync($"{ROUTE}/delete/{id}", cancellationToken);
    }

    public async Task AssociatePermissionsToRoleAsync(Guid roleId, IEnumerable<Guid> permissionIds, CancellationToken cancellationToken)
    {
        var payload = new { RoleId = roleId, PermissionIds = permissionIds };
        await httpClient.PostAsJsonAsync($"{ROUTE}/associate/bulk", payload, cancellationToken);
    }
}
