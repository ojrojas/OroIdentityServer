// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using OroIdentity.Web.Server.Models;
namespace OroIdentity.Web.Server.Services;

public interface ILoginService
{
    Task<HttpResponseMessage> LoginRequest(LoginInputModel loginModel, CancellationToken cancellationToken);
}
