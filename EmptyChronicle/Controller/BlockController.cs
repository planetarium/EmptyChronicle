using EmptyChronicle.Controller.Dto;
using Libplanet;
using Libplanet.Blockchain;
using Libplanet.Blocks;
using Libplanet.Store;
using Libplanet.Tx;
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

    [HttpGet("{index:long}")]
    public ActionResult<string> Get(long index)
    {
        var block = BlockChain[index];
        if (block is null) return NotFound();

        return Ok(new BlockDto(block));
    }
}