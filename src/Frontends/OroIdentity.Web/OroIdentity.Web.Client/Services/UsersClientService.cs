// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using System.Net.Http.Json;
using OroIdentity.Web.Client.Interfaces;
using OroIdentity.Web.Client.Models;

namespace OroIdentity.Web.Client.Services;

internal sealed class UsersClientService(
    ILogger<UsersClientService> logger,
    HttpClient httpClient) : IUsersService
{
    private readonly string ROUTEUSERS = "api/v1/users";

    public async Task CreateUserAsync(UserViewModel user, CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating user with name: {UserName}", user.UserName);
        await httpClient.PostAsJsonAsync(ROUTEUSERS, user, cancellationToken);
        logger.LogInformation("User with name: {UserName} created successfully", user.UserName);
    }

    public async Task DeleteUserAsync(string userId, CancellationToken cancellationToken)
    {
        logger.LogInformation("Deleting user with ID: {UserId}", userId);
        await httpClient.DeleteAsync($"{ROUTEUSERS}/{userId}", cancellationToken);
        logger.LogInformation("User with ID: {UserId} deleted successfully", userId);
    }

    public async Task<BaseResponseViewModel<IEnumerable<UserViewModel>>> GetAllUsersAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving all users");
        var response = await httpClient.GetFromJsonAsync<BaseResponseViewModel<IEnumerable<UserViewModel>>>(
            ROUTEUSERS, cancellationToken);
        return response;
    }

    public async Task<UserViewModel> GetUserByIdAsync(string userId, CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving user with ID: {UserId}", userId);
        return await httpClient.GetFromJsonAsync<UserViewModel>($"{ROUTEUSERS}/{userId}", cancellationToken)
               ?? new UserViewModel();
    }

    public async Task UpdateUserAsync(UserViewModel user, CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating user with ID: {UserId}", user.UserName);
        await httpClient.PutAsJsonAsync(ROUTEUSERS, user, cancellationToken);
        logger.LogInformation("User with ID: {UserId} updated successfully", user.UserName);
    }
}