using OroIdentity.Web.Client.Constants;
using OroIdentity.Web.Client.Interfaces;
using OroIdentity.Web.Client.Models;

namespace OroIdentity.Web.Server.Services;

public class ApplicationsService(
    ILogger<ApplicationsService> logger,
    IHttpClientFactory httpClientFactory) : IApplicationsService
{
    public Task CreateApplicationAsync(ApplicationViewModel application, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<ApplicationViewModel>?> GetAllApplicationAsync(CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Request api external getallapplications");
            using var request = new HttpRequestMessage(HttpMethod.Get, "/applications");
            var client = httpClientFactory.CreateClient(OroIdentityWebConstants.OroIdentityServerApis);
            using var response = await client.SendAsync(request, cancellationToken);
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
            var client = httpClientFactory.CreateClient(OroIdentityWebConstants.OroIdentityServerApis);
            using var response = await client.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<ApplicationViewModel>(cancellationToken: cancellationToken) ??
            throw new IOException("Error request getallapplications");

            return result;
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message, ex);
        }
    }

    public Task UpdateApplicationAsync(ApplicationViewModel application, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}