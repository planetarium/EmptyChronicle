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

    public State? GetStateByAddress(string address) => GetStateByAddress(address, ReservedAddresses.LegacyAccount.ToHex());

    public State? GetStateByAddress(string address, string accountAddress)
    {
        var account = new Address(accountAddress);

        var value = BlockChain
            .GetWorldState(BlockChain.Tip.Hash)
            .GetAccountState(account)
            .GetState(new Address(address));

        return value is null ? null : new State(address, accountAddress, value);
    }
}
