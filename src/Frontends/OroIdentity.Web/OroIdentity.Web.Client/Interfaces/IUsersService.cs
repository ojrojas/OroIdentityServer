using OroIdentity.Web.Client.Models;

namespace OroIdentity.Web.Client.Interfaces;

public interface IUsersService
{
    Task<BaseResponseViewModel<IEnumerable<UserViewModel>>> GetAllUsersAsync(CancellationToken cancellationToken);
    Task<UserViewModel> GetUserByIdAsync(string userId, CancellationToken cancellationToken);
    Task CreateUserAsync(UserViewModel user, CancellationToken cancellationToken);
    Task UpdateUserAsync(UserViewModel user, CancellationToken cancellationToken);
    Task DeleteUserAsync(string userId, CancellationToken cancellationToken);
}