// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
using OroIdentity.Web.Client.Interfaces;
using OroIdentity.Web.Client.Models;


namespace OroIdentity.Web.Client.Services;

public class ApplicationsClientService(
    ILogger<ApplicationsClientService> logger,
    HttpClient httpClient
    // ,AuthenticationState authenticationState
    ) : IApplicationsService
{
    public string APPLICATIONROUTE = "/applications";
    public async Task<IEnumerable<ApplicationViewModel>?> GetAllApplicationAsync(CancellationToken cancellationToken)
    {
        // var auth = authenticationState.User.Identity.IsAuthenticated;
        logger.LogInformation("Request get all applications");
        return await httpClient.GetFromJsonAsync<IEnumerable<ApplicationViewModel>>(APPLICATIONROUTE, cancellationToken);
    }

    public async Task<ApplicationViewModel> GetApplicationByClientIdAsync(Guid ClientId, CancellationToken cancellationToken)
    {
        logger.LogInformation("Request get application by id: {Id}", ClientId);
        Uri uri = new($"{APPLICATIONROUTE}/{ClientId}");
        return await httpClient.GetFromJsonAsync<ApplicationViewModel>(uri, cancellationToken);
    }

    public async Task CreateApplicationAsync(ApplicationViewModel application, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Request create application");
            var response = await httpClient.PostAsync(APPLICATIONROUTE, new StringContent(JsonSerializer.Serialize(application)), cancellationToken);
            response.EnsureSuccessStatusCode();
            logger.LogInformation("Request create application success");

        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message, ex);
        }
    }

    public async Task UpdateApplicationAsync(ApplicationViewModel application, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Request update application");
            var response = await httpClient.PatchAsync(APPLICATIONROUTE, new StringContent(JsonSerializer.Serialize(application)), cancellationToken);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message, ex);
        }
    }
}