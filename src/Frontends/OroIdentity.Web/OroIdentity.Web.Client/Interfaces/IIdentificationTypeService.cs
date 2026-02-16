using OroIdentity.Web.Client.Models;

namespace OroIdentity.Web.Client.Interfaces;


public interface IIdentificationTypeService
{
    Task<BaseResponseViewModel<IEnumerable<IdentificationTypeViewModel>>> GetAllIdentificationTypesAsync(CancellationToken cancellationToken);
    Task<BaseResponseViewModel<IdentificationTypeViewModel>> GetIdentificationTypeByIdAsync(string id, CancellationToken cancellationToken);
    Task CreateIdentificationTypeAsync(IdentificationTypeViewModel identificationType, CancellationToken cancellationToken);
    Task UpdateIdentificationTypeAsync(IdentificationTypeViewModel identificationType, CancellationToken cancellationToken);
    Task DeleteIdentificationTypeAsync(string id, CancellationToken cancellationToken);
}