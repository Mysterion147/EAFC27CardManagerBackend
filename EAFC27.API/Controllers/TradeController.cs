using EAFC27.Application.DTOs;
using EAFC27.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EAFC27.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TradesController : ControllerBase
{
    private readonly ITradeService _tradeService;

    public TradesController(ITradeService tradeService)
    {
        _tradeService = tradeService;
    }

    private Guid GetUserId()
    {
        var claimValue = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return Guid.TryParse(claimValue, out var userId) ? userId : Guid.Empty;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TradeResponseDto>>> GetTrades()
    {
        var userId = GetUserId();
        var trades = await _tradeService.GetUserTradesAsync(userId);
        return Ok(trades);
    }

    [HttpPost]
    public async Task<ActionResult<TradeResponseDto>> AddTrade([FromBody] CreateTradeDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var userId = GetUserId();
            var result = await _tradeService.AddTradeAsync(userId, dto);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<TradeResponseDto>> UpdateTrade(Guid id, [FromBody] CreateTradeDto dto)
    {
        try
        {
            var userId = GetUserId();
            var result = await _tradeService.UpdateTradeAsync(userId, id, dto);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteTrade(Guid id)
    {
        try
        {
            var userId = GetUserId();
            await _tradeService.DeleteTradeAsync(userId, id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}