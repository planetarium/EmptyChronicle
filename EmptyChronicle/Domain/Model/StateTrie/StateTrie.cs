using System.Security.Cryptography;
using Libplanet.Common;

namespace EmptyChronicle.Domain.Model.StateTrie;

public class StateTrie
{
    public readonly string StateRootHash;

    public StateTrie(string stateRootHash)
    {
        StateRootHash = stateRootHash;
    }
}