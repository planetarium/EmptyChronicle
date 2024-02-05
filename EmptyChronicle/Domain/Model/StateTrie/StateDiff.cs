using Bencodex.Types;

namespace EmptyChronicle.Domain.Model.StateTrie;

public class StateDiff
{
    public string Path { get; }
    public IValue? BaseState { get; }
    public IValue? ChangedState { get; }

    public StateDiff(string path, IValue? baseState, IValue? changedState)
    {
        Path = path;
        BaseState = baseState;
        ChangedState = changedState;
    }

    public bool IsAddressRelated(string address) => Path.Contains(address);
}
