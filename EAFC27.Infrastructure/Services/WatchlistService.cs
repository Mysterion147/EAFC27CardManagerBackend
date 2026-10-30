using EAFC27.Application.DTOs;
using EAFC27.Application.Interfaces;
using EAFC27.Domain.Entities;
using EAFC27.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EAFC27.Infrastructure.Services;

public class WatchlistService : IWatchlistService
{
    private readonly AppDbContext _context;

    public WatchlistService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<WatchlistItemResponseDto>> GetUserWatchlistAsync(Guid userId)
    {
        return await _context.WatchlistItems
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
    }

    public async Task<WatchlistItemResponseDto> AddToWatchlistAsync(Guid userId, CreateWatchlistDto dto)
    {
        if (dto.TargetBuyPrice >= dto.TargetSellPrice)
            throw new ArgumentException("The target buy price must be less than the target sell price.");

        var card = await _context.Cards.FindAsync(dto.CardId);
        if (card == null)
            throw new KeyNotFoundException("Card not found in the catalog.");

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

        return new WatchlistItemResponseDto
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
    }

    public async Task<WatchlistItemResponseDto> UpdateWatchlistAsync(Guid userId, Guid id, UpdateWatchlistDto dto)
    {
        if (dto.BuyTarget >= dto.SellTarget)
            throw new ArgumentException("The target buy price must be less than the target sell price.");

        var item = await _context.WatchlistItems
            .Include(w => w.Card)
            .FirstOrDefaultAsync(w => w.Id == id && w.UserId == userId);

        if (item == null)
            throw new KeyNotFoundException("Watchlist item not found.");

        item.BuyTarget = dto.BuyTarget;
        item.SellTarget = dto.SellTarget;

        await _context.SaveChangesAsync();

        return new WatchlistItemResponseDto
        {
            Id = item.Id,
            CardId = item.CardId,
            CardName = item.Card.Name,
            Overall = item.Card.Overall,
            Position = item.Card.Position,
            Club = item.Card.Club,
            Rarity = item.Card.Rarity,
            TargetBuyPrice = item.BuyTarget,
            TargetSellPrice = item.SellTarget,
            CreatedAt = item.CreatedAt
        };
    }

    public async Task RemoveFromWatchlistAsync(Guid userId, Guid id)
    {
        var item = await _context.WatchlistItems
            .FirstOrDefaultAsync(w => w.Id == id && w.UserId == userId);

        if (item == null)
            throw new KeyNotFoundException("Watchlist item not found.");

        _context.WatchlistItems.Remove(item);
        await _context.SaveChangesAsync();
    }
}