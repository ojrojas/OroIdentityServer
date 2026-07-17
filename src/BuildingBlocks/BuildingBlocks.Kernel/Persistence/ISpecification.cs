using System.Linq.Expressions;

namespace BuildingBlocks.Kernel.Persistence;

public interface ISpecification<T>
{
    Expression<Func<T, bool>> ToExpression();
    bool IsSatisfiedBy(T entity);
}
