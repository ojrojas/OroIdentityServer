using System.Text.Json;
using System.Text.Json.Serialization;
using OroIdentity.Web.Client.Interfaces;
using OroIdentity.Web.Client.Models;

namespace OroIdentity.Web.Server.Services;

public class TenantsService(
    ILogger<TenantsService> logger,
    HttpClient httpClient) : ITenantsService
{
     private readonly JsonSerializerOptions options = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    private readonly string ROUTE = "api/tenants";

    public async Task<BaseResponseViewModel<IEnumerable<TenantViewModel>>> GetAllTenantsAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Request api external getalltenants");
        using var response = await httpClient.GetAsync(ROUTE, cancellationToken);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<BaseResponseViewModel<IEnumerable<TenantViewModel>>>(json, options)
            ?? new BaseResponseViewModel<IEnumerable<TenantViewModel>>() { Data = Array.Empty<TenantViewModel>() };
    }

    public async Task<TenantViewModel?> GetTenantByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            return await httpClient.GetFromJsonAsync<TenantViewModel>($"{ROUTE}/{id}", cancellationToken);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task CreateTenantAsync(CreateTenantRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating tenant {Name}", request.Name);
        var body = new { name = new { value = request.Name } };
        var resp = await httpClient.PostAsJsonAsync(ROUTE, body, cancellationToken);
        resp.EnsureSuccessStatusCode();
    }

    public async Task UpdateTenantAsync(Guid id, UpdateTenantRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating tenant {Id}", id);
        var body = new { id = new { value = id }, name = new { value = request.Name }, isActive = request.IsActive };
        var resp = await httpClient.PutAsJsonAsync($"{ROUTE}/{id}", body, cancellationToken);
        resp.EnsureSuccessStatusCode();
    }

    public async Task DeleteTenantAsync(Guid id, CancellationToken cancellationToken)
    {
        logger.LogInformation("Deleting tenant {Id}", id);
        var resp = await httpClient.DeleteAsync($"{ROUTE}/{id}", cancellationToken);
        resp.EnsureSuccessStatusCode();
    }
}
