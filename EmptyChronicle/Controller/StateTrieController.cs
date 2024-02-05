using EmptyChronicle.Application.StateTrie;
using EmptyChronicle.Utility;
using Microsoft.AspNetCore.Mvc;

namespace EmptyChronicle.Controller;

[Route("/api/state-trie")]
[ApiController]
public class StateTrieController : ControllerBase
{
    private readonly StateTrieApplication StateTrieApplication;

    public StateTrieController(StateTrieApplication stateTrieApplication)
    {
        StateTrieApplication = stateTrieApplication;
    }

    [HttpGet("diff")]
    public ActionResult GetStateTrieDiffs(
        [FromQuery(Name = "base")] long? baseIndex,
        [FromQuery(Name = "changed")] long? changedIndex)
    {
        var (actualBaseIndex, actualChangedIndex, diffs)
            = StateTrieApplication.GetStateDiffs(baseIndex, changedIndex);

        if (diffs is null) return NotFound();

        return Ok(new
        {
            BaseIndex = actualBaseIndex,
            ChangedIndex = actualChangedIndex,
            paths = diffs.Select(diff => diff.Path),
        });
    }
    
    [HttpGet("diff/{address}")]
    public ActionResult GetStateTrieDiffByAddress(
        string address,
        [FromQuery(Name = "base")] long? baseIndex,
        [FromQuery(Name = "changed")] long? changedIndex)
    {
        var (actualBaseIndex, actualChangedIndex, diff)
            = StateTrieApplication.GetStateDiffWithAddress(baseIndex, changedIndex, address);

        if (diff is null) return NotFound();

        return Ok(new
        {
            BaseIndex = actualBaseIndex,
            ChangedIndex = actualChangedIndex,
            value = new
            {
                diff.Path,
                BaseState = diff.BaseState?.ToJson(),
                ChangedState = diff.ChangedState?.ToJson(),
            }
        });
    }
}
