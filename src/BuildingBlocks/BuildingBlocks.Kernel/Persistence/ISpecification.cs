using System.Linq.Expressions;

namespace BuildingBlocks.Kernel.Persistence;

public interface ISpecification<T>
{
    Expression<Func<T, bool>> ToExpression();
    bool IsSatisfiedBy(T entity);

    IReadOnlyList<Expression<Func<T, object>>> Includes { get; }
    IReadOnlyList<string> IncludeStrings { get; }

    Expression<Func<T, object>>? OrderBy { get; }
    Expression<Func<T, object>>? OrderByDescending { get; }

    int? Skip { get; }
    int? Take { get; }

    bool AsNoTracking { get; }
    bool IgnoreQueryFilters { get; }
}
