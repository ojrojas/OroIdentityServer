using System.Net.Http.Json;
using OroIdentity.Web.Client.Interfaces;
using OroIdentity.Web.Client.Models;

namespace OroIdentity.Web.Client.Services;

internal sealed class TenantsClientService(
    ILogger<TenantsClientService> logger,
    HttpClient httpClient) : ITenantsService
{
    private readonly string ROUTETENANTS = "api/tenants";

    public async Task<BaseResponseViewModel<IEnumerable<TenantViewModel>>> GetAllTenantsAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving tenants");
        var response = await httpClient.GetFromJsonAsync<BaseResponseViewModel<IEnumerable<TenantViewModel>>>(ROUTETENANTS, cancellationToken: cancellationToken);
        return response ?? new BaseResponseViewModel<IEnumerable<TenantViewModel>>() { Data = Enumerable.Empty<TenantViewModel>() };
    }

    public async Task<TenantViewModel?> GetTenantByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving tenant {Id}", id);
        try
        {
            return await httpClient.GetFromJsonAsync<TenantViewModel>($"{ROUTETENANTS}/{id}", cancellationToken: cancellationToken);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task CreateTenantAsync(CreateTenantRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating tenant {Name}", request.Name);
        var resp = await httpClient.PostAsJsonAsync(ROUTETENANTS, request, cancellationToken);
        resp.EnsureSuccessStatusCode();
    }

    public async Task UpdateTenantAsync(Guid id, UpdateTenantRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating tenant {Id}", id);
        var resp = await httpClient.PutAsJsonAsync($"{ROUTETENANTS}/{id}", request, cancellationToken);
        resp.EnsureSuccessStatusCode();
    }

    public async Task DeleteTenantAsync(Guid id, CancellationToken cancellationToken)
    {
        logger.LogInformation("Deleting tenant {Id}", id);
        var resp = await httpClient.DeleteAsync($"{ROUTETENANTS}/{id}", cancellationToken);
        resp.EnsureSuccessStatusCode();
    }
}
