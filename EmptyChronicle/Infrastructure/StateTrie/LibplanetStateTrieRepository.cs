using System.Security.Cryptography;
using System.Text;
using EmptyChronicle.Domain.Model.StateTrie;
using Libplanet.Blockchain;
using Libplanet.Common;
using Libplanet.Store;
using DomainStateTrie = EmptyChronicle.Domain.Model.StateTrie.StateTrie;

namespace EmptyChronicle.Infrastructure.StateTrie;

public class LibplanetStateTrieRepository : IStateTrieRepository
{
    private readonly BlockChain BlockChain;
    private readonly IStateStore StateStore;

    public LibplanetStateTrieRepository(BlockChain blockChain, IStateStore stateStore)
    {
        BlockChain = blockChain;
        StateStore = stateStore;
    }

    public long GetLatestBlockIndex()
    {
        return BlockChain.Tip.Index;
    }

    public DomainStateTrie? GetStateTrieByBlockIndex(long blockIndex)
    {
        var block = BlockChain[blockIndex];

        return new DomainStateTrie(block.StateRootHash.ToString());
    }

    public StateDiff[] CompareStateTrie(DomainStateTrie baseTrie, DomainStateTrie targetTrie)
    {
        var baseStateRootHash = HashDigest<SHA256>.FromString(baseTrie.StateRootHash);
        var targetStateRootHash = HashDigest<SHA256>.FromString(targetTrie.StateRootHash);

        var baseTrieModel = StateStore.GetStateRoot(baseStateRootHash);
        var targetTrieModel = StateStore.GetStateRoot(targetStateRootHash);

        var diffs = baseTrieModel.Diff(targetTrieModel)
            .Select(diff => new StateDiff(
                Encoding.Default.GetString(diff.Path.ByteArray.ToArray()),
                diff.SourceValue,
                diff.TargetValue))
            .ToArray();

        return diffs;
    }
}
