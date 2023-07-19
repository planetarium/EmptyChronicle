using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Bencodex.Json;
using Libplanet;
using Libplanet.Blocks;

namespace EmptyChronicle.Controller.Dto;

public class BlockDto
{
    public string Hash { get; init; }
    public long Index { get; init; }

    public long TransactionCount => Transactions.Length;
    public DateTimeOffset Timestamp { get; init; }
    public string Miner { get; init; }
    public string StateRootHash { get; init; }
    public TransactionDto[] Transactions { get; init; }

    public BlockDto(Block block)
    {
        Hash = block.Hash.ToString();
        Index = block.Index;
        Miner = block.Miner.ToString();
        Timestamp = block.Timestamp;
        StateRootHash = block.StateRootHash.ToString();
        Transactions = block.Transactions.Select(tx => new TransactionDto
        {
            BlockIndex = block.Index,
            Signature = ByteUtil.Hex(tx.Signature),
            UpdatedAddresses = tx.UpdatedAddresses.Select(x => x.ToString()).ToArray(),
            Id = tx.Id.ToString(),
            PublicKey = tx.PublicKey.ToString(),
            Actions = tx.Actions.Select(action =>
            {
                var converter = new BencodexJsonConverter();
                var buffer = new MemoryStream();
                var writer = new Utf8JsonWriter(buffer);
                converter.Write(writer, action, new JsonSerializerOptions());
                var json = Encoding.UTF8.GetString(buffer.ToArray());

                return JsonNode.Parse(json) ?? JsonNode.Parse("null")!;
            }).ToArray(),
            Nonce = tx.Nonce,
            Signer = tx.Signer.ToString(),
            Timestamp = tx.Timestamp
        }).ToArray();
    }
}