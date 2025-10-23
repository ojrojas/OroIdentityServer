
namespace OroIdentityServer.OroIdentityServer.Infraestructure.Repositories;

public class RolesRepository :  IRolesRepository
{
    public Task AddAsync(Role entity)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(Role entity)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Role>> FindAsync(Expression<Func<Role, bool>> predicate)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Role>> GetAllAsync()
    {
        throw new NotImplementedException();
    }

    public Task<Role?> GetByIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task UpdateAsync(Role entity)
    {
        throw new NotImplementedException();
    }
}