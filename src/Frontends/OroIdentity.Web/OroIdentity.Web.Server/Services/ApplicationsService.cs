using System.Text.Json;
using System.Text.Json.Serialization;
using OroIdentity.Web.Client.Interfaces;
using OroIdentity.Web.Client.Models;

namespace OroIdentity.Web.Server.Services;

public class ApplicationsService(
    ILogger<ApplicationsService> logger,
    HttpClient httpClient) : IApplicationsService
{

    private readonly JsonSerializerOptions options = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public async Task CreateApplicationAsync(ApplicationViewModel application, CancellationToken cancellationToken)
    {
        logger.LogInformation("Request api external createapplication");
        var newApplication = new
        {
            Descriptor = application
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, "applications")
        {
            Content = JsonContent.Create(newApplication)
        };

        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        logger.LogInformation("Application created with ClientId: {ClientId}", newApplication.Descriptor.ClientId);
    }

    public async Task<IEnumerable<ApplicationViewModel>?> GetAllApplicationAsync(CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Request api external getallapplications");
            using var request = new HttpRequestMessage(HttpMethod.Get, "/applications");
            using var response = await httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<IEnumerable<ApplicationViewModel>>(cancellationToken: cancellationToken) ??
            throw new IOException("Error request getallapplications");

            return result;
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message, ex);
        }
    }

    public async Task<ApplicationViewModel> GetApplicationByClientIdAsync(string ClientId, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Request api external getallapplications");
            using var request = new HttpRequestMessage(HttpMethod.Get, $"/applications/{ClientId}");
            using var response = await httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var model = await response.Content.ReadAsStreamAsync(cancellationToken);
            var result = await JsonSerializer.DeserializeAsync<ApplicationViewModel>(model, options, cancellationToken) ??
            throw new IOException("Error request getallapplications");

            return result;
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message, ex);
        }
    }

    public async Task UpdateApplicationAsync(ApplicationViewModel application, CancellationToken cancellationToken)
    {
        logger.LogInformation("Request api external updateapplication for ClientId: {ClientId}", application.ClientId);
        var newApplication = new
        {
            Descriptor = application
        };

        using var request = new HttpRequestMessage(HttpMethod.Patch, $"/applications/{application.ClientId}")
        {
            Content = JsonContent.Create(newApplication)
        };

        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        logger.LogInformation("Application updated with ClientId: {ClientId}", newApplication.Descriptor.ClientId);
    }
}