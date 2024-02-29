namespace EmptyChronicle.Domain.Model.States;

public interface IStatesRepository
{
    State? GetStateByAddress(string address, long? blockIndex=null);
    State? GetStateByAddress(string address, string accountAddress, long? blockIndex=null);
}
