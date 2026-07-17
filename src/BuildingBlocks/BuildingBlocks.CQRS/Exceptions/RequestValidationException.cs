using BuildingBlocks.CQRS.Abstractions;

namespace BuildingBlocks.CQRS.Exceptions;

public sealed class RequestValidationException : Exception
{
    public IReadOnlyCollection<ValidationFailure> Failures { get; }

    public RequestValidationException(IReadOnlyCollection<ValidationFailure> failures)
        : base("One or more validation failures occurred.")
    {
        Failures = failures;
    }
}
