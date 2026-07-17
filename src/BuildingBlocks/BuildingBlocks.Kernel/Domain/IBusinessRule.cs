namespace BuildingBlocks.Kernel.Domain;

public interface IBusinessRule
{
    bool IsBroken();
    string Message { get; }
}
