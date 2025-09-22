using Microsoft.AspNetCore.Mvc;
using TradeExecutor.api.Models;
using TradeExecutor.api.Services;

namespace TradeExecutor.api.Controllers;

[Route("trades")]
public class TradeController(
    ILogger<TradeController> logger,
    ITradeService tradeService
    ) : Controller
{
    [HttpPost("add")]
    public async Task<IActionResult> Add([FromBody] TradeRequestModel tradeRequest)
    {
        var trade = await tradeService.Add(tradeRequest);
        logger.LogInformation("Trade: '{TradeId}' was executed successfully.", trade.Id);
        return Ok(trade.Id);
    }
    
    [HttpPost("update/{id}")]
    public async Task<IActionResult> Update([FromRoute] string id, [FromBody] TradeRequestModel tradeRequest)
    {
        if (string.IsNullOrEmpty(id))
            return BadRequest("'Trade Id' is required to update an existing trade.");
        
        tradeRequest.Id = id;
        var trade = await tradeService.Update(tradeRequest);
        logger.LogInformation("Trade: '{TradeId}' was updated successfully.", trade.Id);
        return Ok(trade.Id);
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<Trade?>> Get([FromRoute] string id)
    {
        var trade = await tradeService.Get(id);
        return Ok(trade);
    }
    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Trade>>> GetAll()
    {
        var trades = await tradeService.GetAll();
        return Ok(trades);
    }
}