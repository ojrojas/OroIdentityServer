namespace OroIdentity.Web.Client.Models;

public record CreateUserRequest(string Name, string MiddleName, string LastName, string UserName, string Email, string Identification, Guid IdentificationTypeId, string Password);

public record UpdateUserRequest(string Name, string MiddleName, string LastName, string UserName, string Email, string Identification, Guid IdentificationTypeId);

public record GetUsersResponse(IEnumerable<UserViewModel>? Data = null);

public record GetUserResponse(UserViewModel? Data = null);

public record CreateUserResponse(Guid? Id = null);

public record UpdateUserResponse;