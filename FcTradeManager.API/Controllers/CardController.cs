using EAFC27.Application.DTOs;
using FcTradeManager.Application.DTOs;
using FcTradeManager.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FcTradeManager.API.Controllers;

[Authorize]
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
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<PagedResultDto<CardResponseDto>>> GetCards(
        [FromQuery] string? name,
        [FromQuery] int? overall,
        [FromQuery] string? position,
        [FromQuery] string? rarity,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        if (page < 1 || pageSize < 1 || pageSize > 100)
            return BadRequest(new { message = "page must be >= 1 and pageSize must be between 1 and 100." });

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

        var totalItems = await query.CountAsync();

        var items = await query
            .OrderBy(c => c.Name)
            .ThenBy(c => c.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
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

        return Ok(new PagedResultDto<CardResponseDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems
        });
    }

    // GET api/cards/{id}
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CardResponseDto>> GetCardById(Guid id)
    {
        var card = await _context.Cards
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);

        if (card == null)
            return NotFound(new { message = "Card not found." });

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