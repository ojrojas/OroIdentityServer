using System.Net.Http.Json;
using OroIdentity.Web.Client.Interfaces;
using OroIdentity.Web.Client.Models;

namespace OroIdentity.Web.Client.Services;

internal sealed class ScopesClientService(
    ILogger<ScopesClientService> logger,
    HttpClient httpClient) : IScopesService
{
    private readonly string ROUTESCOPES = "api/v1/scopes";

    public async Task CreateScopeAsync(ScopeViewModel scope, CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating scope with name: {ScopeName}", scope.Name);
        await httpClient.PostAsJsonAsync(ROUTESCOPES, scope, cancellationToken);
        logger.LogInformation("Scope with name: {ScopeName} created successfully", scope.Name);
    }

    public async Task DeleteScopeAsync(string scopeId, CancellationToken cancellationToken)
    {
        logger.LogInformation("Deleting scope with ID: {ScopeId}", scopeId);
        await httpClient.DeleteAsync($"{ROUTESCOPES}/{scopeId}", cancellationToken);
        logger.LogInformation("Scope with ID: {ScopeId} deleted successfully", scopeId);
    }

    public async Task<BaseResponseViewModel<IEnumerable<ScopeViewModel>>> GetAllScopesAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving all scopes");
        var response = await httpClient.GetFromJsonAsync<BaseResponseViewModel<IEnumerable<ScopeViewModel>>>(
            ROUTESCOPES, cancellationToken);
        return response;
    }

    public async Task<ScopeViewModel> GetScopeByIdAsync(string scopeId, CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving scope with ID: {ScopeId}", scopeId);
        return await httpClient.GetFromJsonAsync<ScopeViewModel>($"{ROUTESCOPES}/{scopeId}", cancellationToken)
               ?? new ScopeViewModel() { Name = string.Empty };
    }

    public async Task<ScopeViewModel> GetScopeByNameAsync(string scopeName, CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving scope with name: {ScopeName}", scopeName);
        return await httpClient.GetFromJsonAsync<ScopeViewModel>($"{ROUTESCOPES}/{scopeName}", cancellationToken)
               ?? new ScopeViewModel() { Name = string.Empty };
    }

    public async Task UpdateScopeAsync(ScopeViewModel scope, CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating scope with ID: {ScopeId}", scope.Id);
        await httpClient.PutAsJsonAsync(ROUTESCOPES, scope, cancellationToken);
        logger.LogInformation("Scope with ID: {ScopeId} updated successfully", scope.Id);
    }
}