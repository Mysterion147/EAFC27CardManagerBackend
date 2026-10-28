using System.Security.Claims;
using EAFC27.Application.DTOs;
using EAFC27.Domain.Entities;
using EAFC27.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EAFC27.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class WatchlistController : ControllerBase
{
    private readonly AppDbContext _context;

    public WatchlistController(AppDbContext context)
    {
        _context = context;
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

        var items = await _context.WatchlistItems
            .Where(w => w.UserId == userId)
            .Include(w => w.Card)
            .AsNoTracking()
            .Select(w => new WatchlistItemResponseDto
            {
                Id = w.Id,
                CardId = w.CardId,
                CardName = w.Card.Name,
                Overall = w.Card.Overall,
                Position = w.Card.Position,
                Club = w.Card.Club,
                Rarity = w.Card.Rarity,
                TargetBuyPrice = w.BuyTarget,
                TargetSellPrice = w.SellTarget,
                CreatedAt = w.CreatedAt
            })
            .ToListAsync();

        return Ok(items);
    }

    // POST: api/Watchlist
    [HttpPost]
    public async Task<ActionResult<WatchlistItemResponseDto>> AddToWatchlist([FromBody] CreateWatchlistDto dto)
    {
        var userId = GetUserId();

        // Verifica se a carta existe no catálogo
        var card = await _context.Cards.FindAsync(dto.CardId);
        if (card == null)
            return NotFound(new { message = "Carta não encontrada no catálogo." });

        // Verifica se já existe na watchlist do utilizador
        var alreadyExists = await _context.WatchlistItems
            .AnyAsync(w => w.UserId == userId && w.CardId == dto.CardId);

        if (alreadyExists)
            return BadRequest(new { message = "Esta carta já se encontra na sua watchlist." });

        var watchlistItem = new WatchlistItem
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CardId = dto.CardId,
            BuyTarget = dto.TargetBuyPrice,
            SellTarget = dto.TargetSellPrice,
            CreatedAt = DateTime.UtcNow
        };

        await _context.WatchlistItems.AddAsync(watchlistItem);
        await _context.SaveChangesAsync();

        var response = new WatchlistItemResponseDto
        {
            Id = watchlistItem.Id,
            CardId = card.Id,
            CardName = card.Name,
            Overall = card.Overall,
            Position = card.Position,
            Club = card.Club,
            Rarity = card.Rarity,
            TargetBuyPrice = watchlistItem.BuyTarget,
            TargetSellPrice = watchlistItem.SellTarget,
            CreatedAt = watchlistItem.CreatedAt
        };

        return Ok(response);
    }

    // DELETE: api/Watchlist/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> RemoveFromWatchlist(Guid id)
    {
        var userId = GetUserId();

        var item = await _context.WatchlistItems
            .FirstOrDefaultAsync(w => w.Id == id && w.UserId == userId);

        if (item == null)
            return NotFound(new { message = "Item da watchlist não encontrado." });

        _context.WatchlistItems.Remove(item);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}