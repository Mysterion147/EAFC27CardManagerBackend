using EAFC27.Application.DTOs;
using EAFC27.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EAFC27.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CardsController : ControllerBase
{
    private readonly AppDbContext _context;

    public CardsController(AppDbContext context)
    {
        _context = context;
    }

    // GET api/cards
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CardResponseDto>>> GetCards(
        [FromQuery] string? name,
        [FromQuery] int? overall,
        [FromQuery] string? position,
        [FromQuery] string? rarity)
    {
        var query = _context.Cards.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(name))
        {
            query = query.Where(c => c.Name.ToLower().Contains(name.ToLower()));
        }

        if (overall.HasValue)
        {
            query = query.Where(c => c.Overall >= overall.Value);
        }

        if (!string.IsNullOrWhiteSpace(position))
        {
            query = query.Where(c => c.Position.ToLower() == position.ToLower());
        }

        if (!string.IsNullOrWhiteSpace(rarity))
        {
            query = query.Where(c => c.Rarity.ToLower() == rarity.ToLower());
        }

        var cards = await query
            .Select(c => new CardResponseDto
            {
                Id = c.Id,
                Name = c.Name,
                Overall = c.Overall,
                Position = c.Position,
                Club = c.Club,
                Rarity = c.Rarity,
                ExternalRef = c.ExternalRef
            })
            .ToListAsync();

        return Ok(cards);
    }

    // GET api/cards/{id}
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CardResponseDto>> GetCardById(Guid id)
    {
        var card = await _context.Cards
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);

        if (card == null)
            return NotFound(new { message = "Carta não encontrada." });

        return Ok(new CardResponseDto
        {
            Id = card.Id,
            Name = card.Name,
            Overall = card.Overall,
            Position = card.Position,
            Club = card.Club,
            Rarity = card.Rarity,
            ExternalRef = card.ExternalRef
        });
    }
}