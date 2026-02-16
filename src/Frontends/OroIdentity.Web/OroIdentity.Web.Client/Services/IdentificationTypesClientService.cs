using System.Net.Http.Json;
using OroIdentity.Web.Client.Interfaces;
using OroIdentity.Web.Client.Models;

namespace OroIdentity.Web.Client.Services;

internal sealed class IdentificationTypesClientService(
    ILogger<IdentificationTypesClientService> logger,
    HttpClient httpClient) : IIdentificationTypeService
{
    private readonly string ROUTE = "api/v1/identificationtypes";

    public async Task<BaseResponseViewModel<IEnumerable<IdentificationTypeViewModel>>> GetAllIdentificationTypesAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving all identification types");
        var response = await httpClient.GetFromJsonAsync<BaseResponseViewModel<IEnumerable<IdentificationTypeViewModel>>>(ROUTE, cancellationToken);
        return response;
    }

    public async Task<BaseResponseViewModel<IdentificationTypeViewModel>?> GetIdentificationTypeByIdAsync(string id, CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving identification type with ID: {Id}", id);
        return await httpClient.GetFromJsonAsync<BaseResponseViewModel<IdentificationTypeViewModel>>($"{ROUTE}/{id}", cancellationToken);
              
    }

    public async Task CreateIdentificationTypeAsync(IdentificationTypeViewModel identificationType, CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating identification type: {Name}", identificationType.Name?.Value);
        await httpClient.PostAsJsonAsync(ROUTE, identificationType, cancellationToken);
        logger.LogInformation("Identification type created successfully: {Name}", identificationType.Name?.Value);
    }

    public async Task UpdateIdentificationTypeAsync(IdentificationTypeViewModel identificationType, CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating identification type with ID: {Id}", identificationType.Id?.Value);
        await httpClient.PutAsJsonAsync(ROUTE, identificationType, cancellationToken);
        logger.LogInformation("Identification type updated successfully: {Id}", identificationType.Id?.Value);
    }

    public async Task DeleteIdentificationTypeAsync(string id, CancellationToken cancellationToken)
    {
        logger.LogInformation("Deleting identification type with ID: {Id}", id);
        await httpClient.DeleteAsync($"{ROUTE}/{id}", cancellationToken);
        logger.LogInformation("Identification type deleted: {Id}", id);
    }

   
}
