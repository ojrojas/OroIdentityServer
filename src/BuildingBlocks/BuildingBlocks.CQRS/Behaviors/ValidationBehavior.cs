using BuildingBlocks.CQRS.Abstractions;
using BuildingBlocks.CQRS.Exceptions;

namespace BuildingBlocks.CQRS.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators) => _validators = validators;

    public async Task<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var failures = new List<ValidationFailure>();
        foreach (var validator in _validators)
            failures.AddRange(await validator.ValidateAsync(request, cancellationToken).ConfigureAwait(false));

        if (failures.Count > 0)
            throw new RequestValidationException(failures);

        return await next(cancellationToken).ConfigureAwait(false);
    }
}
