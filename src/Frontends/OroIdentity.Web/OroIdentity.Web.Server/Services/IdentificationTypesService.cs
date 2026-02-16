using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using OroIdentity.Web.Client.Interfaces;
using OroIdentity.Web.Client.Models;

namespace OroIdentity.Web.Server.Services;

public class IdentificationTypesService(
    ILogger<IdentificationTypesService> logger,
    HttpClient httpClient) : IIdentificationTypeService
{
    private readonly JsonSerializerOptions options = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public async Task<BaseResponseViewModel<IEnumerable<IdentificationTypeViewModel>>> GetAllIdentificationTypesAsync(CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Request external api getall identification types");
            using var response = await httpClient.GetAsync("identificationtypes", cancellationToken);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            var result = JsonSerializer.Deserialize<BaseResponseViewModel<IEnumerable<IdentificationTypeViewModel>>>(json, options);
            return result;
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message, ex);
        }
    }

    public async Task<BaseResponseViewModel<IdentificationTypeViewModel>> GetIdentificationTypeByIdAsync(string id, CancellationToken cancellationToken)
    {
        var response = await httpClient.GetFromJsonAsync<BaseResponseViewModel<IdentificationTypeViewModel>>($"identificationtypes/{id}", cancellationToken) ?? throw new Exception("Identification type not found");
        response.Message = "Identification type retrieved successfully";
        return response;
    }

    public async Task CreateIdentificationTypeAsync(IdentificationTypeViewModel identificationType, CancellationToken cancellationToken)
    {
        var newItem = new
        {
            name = identificationType.Name?.Value,
            isActive = identificationType.IsActive
        };

        await httpClient.PostAsJsonAsync("identificationtypes/create", newItem, options, cancellationToken);
    }

    public async Task UpdateIdentificationTypeAsync(IdentificationTypeViewModel identificationType, CancellationToken cancellationToken)
    {
        var updateItem = new
        {
            id = identificationType.Id?.Value,
            name = identificationType.Name?.Value,
            isActive = identificationType.IsActive
        };

        await httpClient.PutAsJsonAsync("identificationtypes/update", updateItem, options, cancellationToken);
    }

    public async Task DeleteIdentificationTypeAsync(string id, CancellationToken cancellationToken)
    {
        await httpClient.DeleteAsync($"identificationtypes/delete/{id}", cancellationToken);
    }
}
