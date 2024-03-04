using EmptyChronicle.Domain.Model.States;
using Libplanet.Action.State;
using Libplanet.Blockchain;
using Libplanet.Crypto;

namespace EmptyChronicle.Infrastructure.States;

public class LibplanetStatesRepository : IStatesRepository
{
    private readonly BlockChain BlockChain;

    public LibplanetStatesRepository(BlockChain blockChain)
    {
        BlockChain = blockChain;
    }

    public State? GetStateByAddress(string address, long? blockIndex = null) =>
        GetStateByAddress(address, ReservedAddresses.LegacyAccount.ToHex(), blockIndex);

    public State? GetStateByAddress(string address, string accountAddress, long? blockIndex = null)
    {
        var account = new Address(accountAddress);
        var block = blockIndex is { } bi ? BlockChain[bi] : BlockChain.Tip;

        var value = BlockChain
            .GetWorldState(block.Hash)
            .GetAccountState(account)
            .GetState(new Address(address));

        return value is null ? null : new State(address, accountAddress, value);
    }
}
