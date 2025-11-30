using OroIdentity.Web.Server.Models;

namespace OroIdentity.Web.Server.Services;

public interface ILoginService
{
    Task LoginRequest(LoginInputModel loginModel);
}
