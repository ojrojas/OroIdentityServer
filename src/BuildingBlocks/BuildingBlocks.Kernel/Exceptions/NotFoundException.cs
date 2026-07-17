namespace BuildingBlocks.Kernel.Exceptions;

public sealed class NotFoundException : DomainException
{
    public NotFoundException(string entity, object id)
        : base("domain.not_found", $"{entity} with id '{id}' was not found.") { }
}
