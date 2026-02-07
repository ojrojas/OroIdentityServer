// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Services.OroIdentityServer.Server.Services;

public interface IAuthorizationService
{
    Task<LoginResponse> AuthorizedAsync(SimpleRequest requested, CancellationToken cancellationToken = default);
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<LogoutResponse> LogoutAsync(SimpleRequest request, CancellationToken cancellationToken = default);
    Task<LoginResponse> GetTokenAsync(SimpleRequest request, CancellationToken cancellationToken);
    Task<IResult> GetUserInfoAsync(SimpleRequest request, CancellationToken cancellationToken = default);
}
