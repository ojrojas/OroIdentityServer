// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using OroIdentity.Web.Client.Models;

namespace OroIdentity.Web.Client.Interfaces;

public interface IApplicationsService
{
    Task CreateApplicationAsync(ApplicationViewModel application, CancellationToken cancellationToken);
    Task<IEnumerable<ApplicationViewModel>?> GetAllApplicationAsync(CancellationToken cancellationToken);
    Task<ApplicationViewModel> GetApplicationByClientIdAsync(string ClientId, CancellationToken cancellationToken);
    Task UpdateApplicationAsync(ApplicationViewModel application, CancellationToken cancellationToken);
}
