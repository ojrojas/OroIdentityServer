using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using OpenIddict.Abstractions;
using OroIdentity.Web.Client.Interfaces;
using OroIdentity.Web.Client.Models;

namespace OroIdentity.Web.Server.Services;

public class ScopesService(
    ILogger<ScopesService> logger,
    HttpClient httpClient) : IScopesService
{
    private readonly JsonSerializerOptions options = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public async Task CreateScopeAsync(ScopeViewModel scope, CancellationToken cancellationToken)
    {
        var newScope = new
        {
            scopeName = scope.Name
        };
        await httpClient.PostAsJsonAsync("scopes", newScope, options, cancellationToken);
    }

    public async Task DeleteScopeAsync(string scopeId, CancellationToken cancellationToken)
    {
        await httpClient.DeleteAsync($"scopes/{scopeId}", cancellationToken);
    }

    public async Task<BaseResponseViewModel<IEnumerable<ScopeViewModel>>> GetAllScopesAsync(CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Request api external getallscopes");
            using var request = new HttpRequestMessage(HttpMethod.Get, "scopes");
            using var response = await httpClient.GetAsync("scopes", cancellationToken);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(cancellationToken);

            var scopes = JsonSerializer.Deserialize<IEnumerable<OpenIddictScopeDescriptor>>(json, options);

            var viewModels = scopes?.Select(s => new ScopeViewModel
            {
                Id = Guid.NewGuid(), // Since OpenIddictScopeDescriptor doesn't have Id, generate one
                Name = s.Name ?? string.Empty,
                Description = s.Description ?? string.Empty
            }) ?? Enumerable.Empty<ScopeViewModel>();

            var result = new BaseResponseViewModel<IEnumerable<ScopeViewModel>>
            {
                Data = viewModels,
                StatusCode = 200,
                Message = string.Empty,
                Errors = []
            };

            return result;
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message, ex);
        }
    }

    public async Task<ScopeViewModel> GetScopeByIdAsync(string scopeId, CancellationToken cancellationToken)
    {
        return await httpClient.GetFromJsonAsync<ScopeViewModel>($"scopes/{scopeId}", cancellationToken)
               ?? new ScopeViewModel() { Name = string.Empty };
    }

    public async Task<ScopeViewModel> GetScopeByNameAsync(string scopeName, CancellationToken cancellationToken)
    {
        return await httpClient.GetFromJsonAsync<ScopeViewModel>($"scopes/{scopeName}", cancellationToken)
               ?? new ScopeViewModel() { Name = string.Empty };
    }

    public async Task UpdateScopeAsync(ScopeViewModel scope, CancellationToken cancellationToken)
    {
        await httpClient.PutAsJsonAsync("scopes", scope, options, cancellationToken);
    }
}