namespace OroIdentityServer.Infraestructure.Specifications;

public sealed class GetRoleByIdIgnoringFiltersSpecification : Specification<Role>
{
    public GetRoleByIdIgnoringFiltersSpecification(RoleId id) : base(x => x.Id == id)
    {
        ApplyIgnoreQueryFilters();
    }
}
