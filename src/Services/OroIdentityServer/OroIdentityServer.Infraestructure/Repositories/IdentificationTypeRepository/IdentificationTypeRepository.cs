// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.

namespace OroIdentityServer.OroIdentityServer.Infraestructure.Repositories;

public class IdentificationTypeRepository(DbContext context)
: IIdentificationTypeRepository
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
