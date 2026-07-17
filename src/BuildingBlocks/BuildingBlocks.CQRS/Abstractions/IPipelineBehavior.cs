namespace BuildingBlocks.CQRS.Abstractions;

public delegate Task<TResponse> RequestHandlerDelegate<TResponse>(CancellationToken cancellationToken);

/// <summary>Pipeline behavior wrapping command/query handlers.</summary>
public interface IPipelineBehavior<in TRequest, TResponse>
{
    Task<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken);
}

/// <summary>Validates a request prior to handler execution.</summary>
public interface IValidator<in T>
{
    Task<IReadOnlyCollection<ValidationFailure>> ValidateAsync(T instance, CancellationToken cancellationToken);
}

public sealed record ValidationFailure(string PropertyName, string ErrorMessage);
