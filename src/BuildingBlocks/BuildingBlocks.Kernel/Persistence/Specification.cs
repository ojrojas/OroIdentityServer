using System.Linq.Expressions;

namespace BuildingBlocks.Kernel.Persistence;

public abstract class Specification<T> : ISpecification<T>
{
    private readonly List<Expression<Func<T, object>>> _includes = [];
    private readonly List<string> _includeStrings = [];

    protected Specification() : this(_ => true)
    {
    }

    protected Specification(Expression<Func<T, bool>> criteria)
    {
        Criteria = criteria;
    }

    public Expression<Func<T, bool>> Criteria { get; protected set; }

    public IReadOnlyList<Expression<Func<T, object>>> Includes => _includes;
    public IReadOnlyList<string> IncludeStrings => _includeStrings;

    public Expression<Func<T, object>>? OrderBy { get; private set; }
    public Expression<Func<T, object>>? OrderByDescending { get; private set; }

    public int? Skip { get; private set; }
    public int? Take { get; private set; }

    public bool AsNoTracking { get; private set; }
    public bool IgnoreQueryFilters { get; private set; }

    protected void AddInclude(Expression<Func<T, object>> includeExpression) => _includes.Add(includeExpression);

    protected void AddInclude(string includeString) => _includeStrings.Add(includeString);

    protected void ApplyOrderBy(Expression<Func<T, object>> orderByExpression) => OrderBy = orderByExpression;

    protected void ApplyOrderByDescending(Expression<Func<T, object>> orderByDescendingExpression) =>
        OrderByDescending = orderByDescendingExpression;

    protected void ApplyPaging(int skip, int take)
    {
        Skip = skip;
        Take = take;
    }

    protected void ApplyTake(int take) => Take = take;

    protected void ApplyNoTracking() => AsNoTracking = true;

    protected void ApplyIgnoreQueryFilters() => IgnoreQueryFilters = true;

    public bool IsSatisfiedBy(T entity) => Criteria.Compile()(entity);

    public Expression<Func<T, bool>> ToExpression() => Criteria;
}
