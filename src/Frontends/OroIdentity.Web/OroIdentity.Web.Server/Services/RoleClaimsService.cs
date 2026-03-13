using System.Text.Json;
using System.Text.Json.Serialization;
using OroIdentity.Web.Client.Interfaces;
using OroIdentity.Web.Client.Models;

namespace OroIdentity.Web.Server.Services;

public class RoleClaimsService(
    ILogger<RoleClaimsService> logger,
    HttpClient httpClient) : IRoleClaimsService
{
    private readonly JsonSerializerOptions options = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    private readonly HttpClient _httpClient = httpClient;
    private readonly string ROUTE = "roleclaims"; // identity server route

    public async Task<RoleClaimViewModel?> GetRoleClaimByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _httpClient.GetFromJsonAsync<RoleClaimViewModel>($"{ROUTE}/get/{id}", cancellationToken);
    }

    public async Task<BaseResponseViewModel<IEnumerable<RoleClaimViewModel>>> GetRoleClaimsByRoleIdAsync(Guid roleId, CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync($"{ROUTE}/getbyrole/{roleId}", cancellationToken);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        var result = JsonSerializer.Deserialize<BaseResponseViewModel<IEnumerable<RoleClaimViewModel>>>(json, options)
                     ?? new BaseResponseViewModel<IEnumerable<RoleClaimViewModel>> { Data = Array.Empty<RoleClaimViewModel>() };
        return result;
    }

    public async Task AssociateClaimToRoleAsync(Guid roleId, string claimType, string claimValue, CancellationToken cancellationToken)
    {
        var payload = new { RoleId = roleId, ClaimType = claimType, ClaimValue = claimValue };
        await _httpClient.PostAsJsonAsync($"{ROUTE}/associate", payload, options, cancellationToken);
    }

    public async Task UpdateRoleClaimAsync(RoleClaimViewModel claim, CancellationToken cancellationToken)
    {
        await _httpClient.PutAsJsonAsync($"{ROUTE}/update", claim, options, cancellationToken);
    }

    public async Task DeleteRoleClaimAsync(Guid id, CancellationToken cancellationToken)
    {
        await _httpClient.DeleteAsync($"{ROUTE}/delete/{id}", cancellationToken);
    }
}
