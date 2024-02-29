using Bencodex.Types;

namespace EmptyChronicle.Domain.Model.States;

public class State
{
    public string Address { get; }
    public string AccountAddress { get; }
    public IValue Value { get; }

    public State(string address, string accountAddress, IValue value)
    {
        Address = address;
        AccountAddress = accountAddress;
        Value = value;
    }
}
