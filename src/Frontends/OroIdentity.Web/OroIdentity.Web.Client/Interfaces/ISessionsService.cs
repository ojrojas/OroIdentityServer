using OroIdentity.Web.Client.Models;

namespace OroIdentity.Web.Client.Interfaces;

public interface ISessionsService
{
    Task<BaseResponseViewModel<IEnumerable<SessionViewModel>>> GetAllSessionsAsync(CancellationToken cancellationToken);
    Task<BaseResponseViewModel<IEnumerable<SessionViewModel>>> GetUserSessionsAsync(Guid userId, CancellationToken cancellationToken);
    Task TerminateSessionAsync(Guid sessionId, CancellationToken cancellationToken);
}
