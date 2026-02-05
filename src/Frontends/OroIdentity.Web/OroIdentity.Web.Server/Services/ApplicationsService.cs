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
        logger.LogInformation("Request api external getallapplications");
        using var request = new HttpRequestMessage(HttpMethod.Get, "/applications");
        var client  = httpClientFactory.CreateClient("OroIdentityServerApis");
        using var response = await  client.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<IEnumerable<ApplicationViewModel>>(cancellationToken: cancellationToken) ?? 
        throw new IOException("Error request getallapplications");
    }

    public Task<ApplicationViewModel> GetApplicationByClientIdAsync(Guid ClientId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task UpdateApplicationAsync(ApplicationViewModel application, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}