using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Bencodex.Json;
using EmptyChronicle.Controller.Dto;
using Libplanet.Common;
using Libplanet.Blockchain;
using Libplanet.Types.Blocks;
using Microsoft.AspNetCore.Mvc;

namespace EmptyChronicle.Controller;

[Route("api/blocks")]
[ApiController]
public class BlockController : ControllerBase
{
    private BlockChain BlockChain { get; init; }

    public BlockController(BlockChain blockChain)
    {
        BlockChain = blockChain;
    }

    [HttpGet("latest")]
    public ActionResult<string> GetLatest()
    {
        var block = BlockChain[BlockChain.Count - 1];
        if (block is null) return NotFound();

        return Ok(GenerateBlockDto(block));
    }

    [HttpGet("{index:long}")]
    public ActionResult<string> Get(long index)
    {
        var block = BlockChain[index];
        if (block is null) return NotFound();

        return Ok(GenerateBlockDto(block));
    }

    public BlockDto GenerateBlockDto(Block block)
    {
        return new BlockDto
        {
            Hash = block.Hash.ToString(),
            Index = block.Index,
            Miner = block.Miner.ToString(),
            Timestamp = block.Timestamp,
            StateRootHash = block.StateRootHash.ToString(),
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
            }).ToArray(),
        };
    }
}
