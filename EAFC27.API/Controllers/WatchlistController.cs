using System.Security.Claims;
using EAFC27.Application.DTOs;
using EAFC27.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EAFC27.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class WatchlistController : ControllerBase
{
    private readonly IWatchlistService _watchlistService;

    // Injeção de dependência do serviço
    public WatchlistController(IWatchlistService watchlistService)
    {
        _watchlistService = watchlistService;
    }

    private Guid GetUserId()
    {
        var claimValue = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return Guid.TryParse(claimValue, out var userId) ? userId : Guid.Empty;
    }

    // GET: api/Watchlist
    [HttpGet]
    public async Task<ActionResult<IEnumerable<WatchlistItemResponseDto>>> GetUserWatchlist()
    {
        var userId = GetUserId();
        var items = await _watchlistService.GetUserWatchlistAsync(userId);
        return Ok(items);
    }

    // POST: api/Watchlist
    [HttpPost]
    public async Task<ActionResult<WatchlistItemResponseDto>> AddToWatchlist([FromBody] CreateWatchlistDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var userId = GetUserId();
            var result = await _watchlistService.AddToWatchlistAsync(userId, dto);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException)
        {
            return Conflict(new { message = "Esta carta já se encontra na sua watchlist." });
        }
    }

    // PUT: api/Watchlist/{id}
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<WatchlistItemResponseDto>> UpdateWatchlist(Guid id, [FromBody] UpdateWatchlistDto dto) // id of the card on the watchlist, not the cardId itself
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var userId = GetUserId();
            var result = await _watchlistService.UpdateWatchlistAsync(userId, id, dto);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // DELETE: api/Watchlist/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> RemoveFromWatchlist(Guid id)
    {
        try
        {
            var userId = GetUserId();
            await _watchlistService.RemoveFromWatchlistAsync(userId, id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}