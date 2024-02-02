using EmptyChronicle.Domain.Model.StateTrie;

namespace EmptyChronicle.Domain.Service;

public class StateTrieService
{
    private readonly IStateTrieRepository StateTrieRepository;

    public StateTrieService(IStateTrieRepository stateTrieRepository)
    {
        StateTrieRepository = stateTrieRepository;
    }

    public StateDiff[]? CompareStateTrie(long blockInterval, StateTrie baseTrie, StateTrie targetTrie)
    {
        switch (blockInterval)
        {
            case <= 0:
                throw new InvalidBlockIndexIntervalException(InvalidBlockIndexIntervalException.ExceptionReason
                    .BaseOverChanged);
            case > 100:
                throw new InvalidBlockIndexIntervalException(InvalidBlockIndexIntervalException.ExceptionReason
                    .IntervalTooLong);
        }

        return StateTrieRepository.CompareStateTrie(baseTrie, targetTrie);
    }
}