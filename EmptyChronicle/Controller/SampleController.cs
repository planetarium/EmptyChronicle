using Libplanet;
using Libplanet.Blockchain;
using Libplanet.Blocks;
using Libplanet.Store;
using Libplanet.Tx;
using Microsoft.AspNetCore.Mvc;

namespace EmptyChronicle.Controller;

[Route("api/blockchain")]
[ApiController]
public class SampleController : ControllerBase
{
    private BlockChain BlockChain { get; init; }
    private IStore Store { get; init; }

    public SampleController(BlockChain blockChain, IStore store)
    {
        BlockChain = blockChain;
        Store = store;
    }

    [HttpGet]
    public ActionResult<string> Get()
    {
        var blockHash = BlockChain.BlockHashes.Last();

        if (BuildBlock(blockHash) is not { } block) return NotFound();

        var previousBlock = block.PreviousHash switch
        {
            { } hash => BuildBlock(hash),
            _ => null,
        };

        return Ok(new BlockChainResponse
        {
            Id = BlockChain.Id.ToString(),
            Count = BlockChain.BlockHashes.Count(),
            Block = new BlockResponse(block)
            {
                PreviousBlock = previousBlock switch
                {
                    not null => new BlockResponse(previousBlock),
                    null => null
                }
            }
        });
    }

    [HttpGet("blocks/{index:int}")]
    public ActionResult<string> GetBlock(int index)
    {
        var blockHash = BlockChain.BlockHashes.ElementAtOrDefault(index);

        if (BuildBlock(blockHash) is not { } block) return NotFound();

        var previousBlock = block.PreviousHash switch
        {
            { } hash => BuildBlock(hash),
            _ => null
        };


        return Ok(new BlockResponse(block)
        {
            PreviousBlock = previousBlock switch
            {
                not null => new BlockResponse(previousBlock),
                _ => null
            }
        });
    }

    [HttpGet("transactions/{txid}")]
    public ActionResult<string> GetTransaction(string txid)
    {
        var tx = Store.GetTransaction(new TxId(ByteUtil.ParseHex(txid)));

        if (tx is null) return NotFound();

        return Ok(new TransactionResponse(tx));
    }

    private Block? BuildBlock(BlockHash hash)
    {
        if (Store.GetBlockDigest(hash) is not { } digest) return null;

        var header = digest.GetHeader();

        (TxId TxId, Transaction Tx)[] txs = digest.TxIds
            .Select(bytes => new TxId(bytes.ToArray()))
            .OrderBy(txid => txid)
            .Select(txid => (txid, Store.GetTransaction(txid)))
            .ToArray();

        return new Block(header, txs.Select(pair => pair.Tx));
    }

    private class BlockChainResponse
    {
        public string? Id { get; init; }
        public long Count { get; init; }

        public BlockResponse? Block { get; init; }
    }

    private class BlockResponse
    {
        public long Index { get; init; }
        public string? Hash { get; init; }
        public string? Miner { get; init; }
        public string? PublicKey { get; init; }
        public BlockResponse? PreviousBlock { get; init; }
        public DateTimeOffset Timestamp { get; init; }
        public string? StateRootHash { get; init; }

        public TransactionResponse[] Transactions { get; init; }

        public long TransactionCount => Transactions.Length;
        public string? Signature { get; init; }

        public BlockResponse(Block block)
        {
            Index = block.Index;
            Hash = block.Hash.ToString();
            Miner = block.Miner.ToString();
            PublicKey = block.PublicKey?.ToString();
            Timestamp = block.Timestamp;
            Transactions = block.Transactions.Select(tx => new TransactionResponse(tx)).ToArray();
            StateRootHash = block.StateRootHash.ToString();
            Signature = block.Signature.ToString();
        }
    }

    private class TransactionResponse
    {
        public string? Id { get; init; }
        public long Nonce { get; init; }
        public string? Signer { get; init; }
        public string? PublicKey { get; init; }
        public string[]? UpdatedAddresses { get; init; }
        public string? Signature { get; init; }
        public DateTimeOffset Timestamp { get; init; }

        public TransactionResponse(ITransaction tx)
        {
            Id = tx.Id.ToString();
            Nonce = tx.Nonce;
            Signer = tx.Signer.ToString();
            PublicKey = tx.PublicKey.ToString();
            UpdatedAddresses = tx.UpdatedAddresses.Select(address => address.ToString()).ToArray();
            Signature = tx.Signature.ToString();
            Timestamp = tx.Timestamp;
        }
    }
}