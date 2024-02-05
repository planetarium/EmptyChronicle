namespace EmptyChronicle.Domain.Model.StateTrie;

public interface IStateTrieRepository
{
    long GetLatestBlockIndex();
    StateTrie? GetStateTrieByBlockIndex(long blockIndex);
    StateDiff[] CompareStateTrie(StateTrie baseTrie, StateTrie targetTrie);
}
