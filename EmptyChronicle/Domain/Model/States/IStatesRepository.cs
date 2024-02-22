namespace EmptyChronicle.Domain.Model.States;

public interface IStatesRepository
{
    State? GetStateByAddress(string address);
    State? GetStateByAddress(string address, string accountAddress);
}