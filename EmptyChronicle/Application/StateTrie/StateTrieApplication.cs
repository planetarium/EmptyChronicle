using EmptyChronicle.Domain.Model.StateTrie;
using EmptyChronicle.Domain.Service;

namespace EmptyChronicle.Application.StateTrie;

public class StateTrieApplication
{
    private readonly IStateTrieRepository StateTrieRepository;
    private readonly StateTrieService StateTrieService;

    public StateTrieApplication(IStateTrieRepository stateTrieRepository)
    {
        StateTrieRepository = stateTrieRepository;
        StateTrieService = new StateTrieService(stateTrieRepository);
    }

    public (long baseIndex, long changedIndex, StateDiff[]? diffs) GetStateDiffs(long? baseIndex, long? changedIndex)
    {
        var filledChangedIndex = baseIndex ?? StateTrieRepository.GetLatestBlockIndex();
        var filledBaseIndex = changedIndex ?? filledChangedIndex - 1;

        var baseTrie = StateTrieRepository.GetStateTrieByBlockIndex(filledBaseIndex);
        var changedTrie = StateTrieRepository.GetStateTrieByBlockIndex(filledChangedIndex);

        if (baseTrie is null || changedTrie is null) return (filledBaseIndex, filledChangedIndex, null);

        var blockInterval = filledChangedIndex - filledBaseIndex;
        var diffs = StateTrieService.CompareStateTrie(blockInterval, baseTrie, changedTrie);

        return (filledBaseIndex, filledChangedIndex, diffs);
    }

    public (long baseIndex, long changedIndex, StateDiff? diff) GetStateDiffWithAddress(
        long? baseIndex,
        long? changedIndex,
        string address)
    {
        var filledChangedIndex = baseIndex ?? StateTrieRepository.GetLatestBlockIndex();
        var filledBaseIndex = changedIndex ?? filledChangedIndex - 1;

        var baseTrie = StateTrieRepository.GetStateTrieByBlockIndex(filledBaseIndex);
        var changedTrie = StateTrieRepository.GetStateTrieByBlockIndex(filledChangedIndex);

        if (baseTrie is null || changedTrie is null) return (filledBaseIndex, filledChangedIndex, null);

        var blockInterval = filledChangedIndex - filledBaseIndex;
        var diffs = StateTrieService.CompareStateTrie(blockInterval, baseTrie, changedTrie);

        var diff = diffs?.FirstOrDefault(diff => diff.Path.Contains(address));

        return (filledBaseIndex, filledChangedIndex, diff);
    }
}