using Bencodex;
using EmptyChronicle.Application.States;
using EmptyChronicle.Utility;
using Microsoft.AspNetCore.Mvc;

namespace EmptyChronicle.Controller;

[Route("/api/states")]
[ApiController]
public class StatesController : ControllerBase
{
    private readonly StatesApplication StatesApplication;

    public StatesController(StatesApplication statesApplication)
    {
        StatesApplication = statesApplication;
    }

    [HttpGet("{address}")]
    public ActionResult GetStatesByAddress(
        string address,
        [FromQuery(Name = "account")] string? accountAddress,
        [FromQuery(Name = "blockIndex")] long? blockIndex
    )
    {
        try
        {
            var state = StatesApplication.GetStateByAddress(address, accountAddress, blockIndex);
            if (state is null)
                return NotFound();

            return Ok(
                new
                {
                    state.Address,
                    state.AccountAddress,
                    Value = state.Value.ToJson()
                }
            );
        }
        catch (ArgumentException)
        {
            return BadRequest();
        }
    }
}
