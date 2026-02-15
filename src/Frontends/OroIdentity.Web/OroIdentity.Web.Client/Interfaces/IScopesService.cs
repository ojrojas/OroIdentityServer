using OroIdentity.Web.Client.Models;

namespace OroIdentity.Web.Client.Interfaces;

public interface IScopesService
{
    Task<BaseResponseViewModel<IEnumerable<ScopeViewModel>>> GetAllScopesAsync(CancellationToken cancellationToken);
    Task<ScopeViewModel> GetScopeByNameAsync(string scopeName, CancellationToken cancellationToken);
    Task<ScopeViewModel> GetScopeByIdAsync(string scopeId, CancellationToken cancellationToken);
    Task CreateScopeAsync(ScopeViewModel scope, CancellationToken cancellationToken);
    Task UpdateScopeAsync(ScopeViewModel scope, CancellationToken cancellationToken);
    Task DeleteScopeAsync(string scopeId, CancellationToken cancellationToken);
}