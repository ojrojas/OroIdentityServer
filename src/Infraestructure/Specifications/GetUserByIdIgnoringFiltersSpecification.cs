namespace OroIdentityServer.Infraestructure.Specifications;

public sealed class GetUserByIdIgnoringFiltersSpecification : Specification<User>
{
    public GetUserByIdIgnoringFiltersSpecification(UserId id) : base(x => x.Id == id)
    {
        ApplyIgnoreQueryFilters();
        AddInclude(x => x.Roles);
        AddInclude("Roles.Role");
        AddInclude(x => x.SecurityUser!);
    }
}
