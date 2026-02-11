// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using System.Net.Http.Json;
using System.Text.Json;
using OroIdentity.Web.Client.Interfaces;
using OroIdentity.Web.Client.Models;

namespace OroIdentity.Web.Client.Services;

internal sealed class ApplicationsClientService(
    HttpClient httpClient
    ) : IApplicationsService
{
    public string APPLICATIONROUTE = "api/v1/applications";
    public async Task<IEnumerable<ApplicationViewModel>?> GetAllApplicationAsync(CancellationToken cancellationToken)
    {
        return await httpClient.GetFromJsonAsync<IEnumerable<ApplicationViewModel>>(APPLICATIONROUTE, cancellationToken);
    }

    public async Task<ApplicationViewModel?> GetApplicationByClientIdAsync(string ClientId, CancellationToken cancellationToken)
    {
        var uri = $"{APPLICATIONROUTE}/{ClientId}";
        return await httpClient.GetFromJsonAsync<ApplicationViewModel>(uri, cancellationToken);
    }

    public async Task CreateApplicationAsync(ApplicationViewModel application, CancellationToken cancellationToken)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync(APPLICATIONROUTE, application, cancellationToken);
            response.EnsureSuccessStatusCode();
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
            var response = await httpClient.PutAsJsonAsync(APPLICATIONROUTE, application, cancellationToken);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message, ex);
        }
    }
}