using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using OroIdentity.Web.Client.Interfaces;
using OroIdentity.Web.Client.Models;

namespace OroIdentity.Web.Server.Services;

public class RolesService(
    ILogger<RolesService> logger,
    HttpClient httpClient) : IRolesService
{
    private readonly JsonSerializerOptions options = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public async Task CreateRoleAsync(RoleViewModel role, CancellationToken cancellationToken)
    {
        var newRole = new
        {
            roleName = role.Name
        };
        await httpClient.PostAsJsonAsync("roles", newRole, options, cancellationToken);
    }

    public async Task DeleteRoleAsync(string roleId, CancellationToken cancellationToken)
    {
        await httpClient.DeleteAsync($"roles/{roleId}", cancellationToken);
    }

    public async Task<BaseResponseViewModel<IEnumerable<RoleViewModel>>> GetAllRolesAsync(CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Request api external getallroles");
            using var request = new HttpRequestMessage(HttpMethod.Get, "roles");
            using var response = await httpClient.GetAsync("roles", cancellationToken);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(cancellationToken);

            var result = JsonSerializer.Deserialize<BaseResponseViewModel<IEnumerable<RoleViewModel>>>(json, options);

            return result;
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message, ex);
        }
    }

    public async Task<RoleViewModel> GetRoleByIdAsync(string roleId, CancellationToken cancellationToken)
    {
        return await httpClient.GetFromJsonAsync<RoleViewModel>($"roles/{roleId}", cancellationToken)
               ?? new RoleViewModel() { Name = NameViewModel.Create(string.Empty) };
    }

    public async Task<RoleViewModel> GetRoleByNameAsync(string roleName, CancellationToken cancellationToken)
    {
        return await httpClient.GetFromJsonAsync<RoleViewModel>($"roles/{roleName}", cancellationToken)
               ?? new RoleViewModel() { Name = NameViewModel.Create(string.Empty) };
    }

    public async Task UpdateRoleAsync(RoleViewModel role, CancellationToken cancellationToken)
    {
        await httpClient.PutAsJsonAsync("roles", role, options, cancellationToken);
    }
}