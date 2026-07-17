using BuildingBlocks.Kernel.Domain;

namespace BuildingBlocks.Kernel.Exceptions;

public sealed class BusinessRuleValidationException : DomainException
{
    public IBusinessRule BrokenRule { get; }

    public BusinessRuleValidationException(IBusinessRule rule)
        : base("domain.rule.broken", rule.Message)
    {
        BrokenRule = rule;
    }

    public override string ToString() => $"{BrokenRule.GetType().FullName}: {BrokenRule.Message}";
}
