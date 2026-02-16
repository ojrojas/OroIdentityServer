using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using OroIdentity.Web.Client.Interfaces;
using OroIdentity.Web.Client.Models;

namespace OroIdentity.Web.Server.Services;

public class UsersService(
    ILogger<UsersService> logger,
    HttpClient httpClient) : IUsersService
{
    private readonly JsonSerializerOptions options = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public async Task CreateUserAsync(UserViewModel user, CancellationToken cancellationToken)
    {
        var newUser = new
        {
            userName = user.UserName,
            name = user.Name,
            middleName = user.MiddleName,
            lastName = user.LastName,
            email = user.Email,
            identification = user.Identification,
            identificationTypeId = user.IdentificationTypeId,
            password = user.Security.PasswordHash
        };
        await httpClient.PostAsJsonAsync("users", newUser, options, cancellationToken);
    }

    public async Task DeleteUserAsync(string userId, CancellationToken cancellationToken)
    {
        await httpClient.DeleteAsync($"users/{userId}", cancellationToken);
    }

    public async Task<BaseResponseViewModel<IEnumerable<UserViewModel>>> GetAllUsersAsync(CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Request api external getallusers");
            using var response = await httpClient.GetAsync("users", cancellationToken);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(cancellationToken);

            var result = JsonSerializer.Deserialize<BaseResponseViewModel<IEnumerable<UserViewModel>>>(json, options);

            return result;
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message, ex);
        }
    }

    public async Task<UserViewModel> GetUserByIdAsync(string userId, CancellationToken cancellationToken)
    {
        return await httpClient.GetFromJsonAsync<UserViewModel>($"users/{userId}", cancellationToken)
               ?? new UserViewModel();
    }

    public async Task UpdateUserAsync(UserViewModel user, CancellationToken cancellationToken)
    {
        await httpClient.PutAsJsonAsync("users", user, options, cancellationToken);
    }
}