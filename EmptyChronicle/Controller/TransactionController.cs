using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Bencodex.Json;
using EmptyChronicle.Controller.Dto;
using Libplanet;
using Libplanet.Blockchain;
using Libplanet.Blocks;
using Libplanet.Store;
using Libplanet.Tx;
using Microsoft.AspNetCore.Mvc;

namespace EmptyChronicle.Controller;

[Route("api/transactions")]
[ApiController]
public class TransactionController : ControllerBase
{
    private BlockChain BlockChain { get; init; }
    private IStore Store { get; init; }

    public TransactionController(BlockChain blockChain, IStore store)
    {
        BlockChain = blockChain;
        Store = store;
    }

    [HttpGet("{stringTxId}")]
    public ActionResult<string> Get(string stringTxId)
    {
        var txId = new TxId(ByteUtil.ParseHex(stringTxId));
        var tx = Store.GetTransaction(txId);
        if (tx is null) return NotFound();

        var nullableBlockHash = Store.GetFirstTxIdBlockHashIndex(txId);
        if (nullableBlockHash is not { } blockHash) return NotFound();

        var blockIndex = GetBlockIndex(blockHash);
        var execution = BlockChain.GetTxExecution(blockHash, txId);
        var isTxStaging = BlockChain.GetStagedTransactionIds().Contains(txId);

        return Ok(new TransactionDetailDto
        {
            Signature = ByteUtil.Hex(tx.Signature),
            Signer = tx.Signer.ToString(),
            PublicKey = tx.PublicKey.ToString(),
            Timestamp = tx.Timestamp,
            Status = execution switch
            {
                TxSuccess => "SUCCESS",
                TxFailure => "FAILURE",
                _ when isTxStaging => "STAGING",
                _ => "INVALID"
            },
            Nonce = tx.Nonce,
            BlockIndex = blockIndex ?? 0,
            Id = tx.Id.ToString(),
            UpdatedAddresses = tx.UpdatedAddresses.Select(x => x.ToString()).ToArray(),
            Actions = tx.Actions.Select(action =>
            {
                var converter = new BencodexJsonConverter();
                var buffer = new MemoryStream();
                var writer = new Utf8JsonWriter(buffer);
                converter.Write(writer, action, new JsonSerializerOptions());
                var json = Encoding.UTF8.GetString(buffer.ToArray());

                return JsonNode.Parse(json) ?? JsonNode.Parse("null")!;
            }).ToArray()
        });
    }

    private long? GetBlockIndex(BlockHash hash)
    {
        if (Store.GetBlockDigest(hash) is not { } digest) return null;

        var header = digest.GetHeader();

        return header.Index;
    }
}