using EAFC27.Application.DTOs;
using EAFC27.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EAFC27.API.Controllers;

[Authorize]
[ApiController]
[Route("api/cards/{cardId:guid}/prices")]
public class PricesController : ControllerBase
{
    private readonly IPriceService _priceService;

    public PricesController(IPriceService priceService)
    {
        _priceService = priceService;
    }

    // GET: api/cards/{cardId}/prices?period=24h
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PriceSnapshotResponseDto>>> GetPriceHistory(
        Guid cardId, 
        [FromQuery] string period = "24h")
    {
        try
        {
            var history = await _priceService.GetPriceHistoryAsync(cardId, period);
            return Ok(history);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // POST: api/cards/{cardId}/prices
    [HttpPost]
    public async Task<ActionResult<PriceSnapshotResponseDto>> AddPriceSnapshot(
        Guid cardId, 
        [FromBody] CreatePriceSnapshotDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var result = await _priceService.AddPriceSnapshotAsync(cardId, dto, source: "manual");
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}