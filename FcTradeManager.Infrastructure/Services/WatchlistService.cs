using FcTradeManager.Application.DTOs;
using FcTradeManager.Application.Interfaces;
using FcTradeManager.Domain.Entities;
using FcTradeManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FcTradeManager.Infrastructure.Services;

public class WatchlistService : IWatchlistService
{
    private readonly AppDbContext _context;

    public WatchlistService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<WatchlistItemResponseDto>> GetUserWatchlistAsync(Guid userId, string? sortBy = null)
    {
        var watchlistItems = await _context.WatchlistItems
            .Where(w => w.UserId == userId)
            .Include(w => w.Card)
            .AsNoTracking()
            .ToListAsync();

        if (watchlistItems.Count == 0)
            return new List<WatchlistItemResponseDto>();

        var cardIds = watchlistItems.Select(w => w.CardId).Distinct().ToList();
        var cutoff24h = DateTime.UtcNow.AddHours(-24);

        // one query: last price, baseline (<= 24h ago), oldest in the window and the window size, per card
        var prices = await _context.Cards
            .Where(c => cardIds.Contains(c.Id))
            .Select(c => new
            {
                CardId = c.Id,
                Last = c.PriceSnapshots
                    .OrderByDescending(p => p.RecordedAt)
                    .Select(p => (int?)p.Price)
                    .FirstOrDefault(),
                Baseline = c.PriceSnapshots
                    .Where(p => p.RecordedAt <= cutoff24h)
                    .OrderByDescending(p => p.RecordedAt)
                    .Select(p => (int?)p.Price)
                    .FirstOrDefault(),
                Oldest = c.PriceSnapshots
                    .Where(p => p.RecordedAt > cutoff24h)
                    .OrderBy(p => p.RecordedAt)
                    .Select(p => (int?)p.Price)
                    .FirstOrDefault(),
                WindowCount = c.PriceSnapshots.Count(p => p.RecordedAt > cutoff24h)
            })
            .AsNoTracking()
            .ToDictionaryAsync(x => x.CardId);

        var result = new List<WatchlistItemResponseDto>();

        foreach (var w in watchlistItems)
        {
            int? lastPrice = null;
            int? baseline = null;

            if (prices.TryGetValue(w.CardId, out var p))
            {
                lastPrice = p.Last;
                baseline = p.Baseline ?? (p.WindowCount > 1 ? p.Oldest : null);
            }

            var (change, changePercent) = ComputeChange(lastPrice, baseline);

            result.Add(new WatchlistItemResponseDto
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
                LastPrice = lastPrice,
                PriceChange24h = change,
                PriceChangePercent24h = changePercent,
                DistanceToBuy = lastPrice.HasValue ? lastPrice.Value - w.BuyTarget : null,
                DistanceToSell = lastPrice.HasValue ? w.SellTarget - lastPrice.Value : null,
                CreatedAt = w.CreatedAt
            });
        }

        return sortBy?.ToLower() switch
        {
            "name" => result.OrderBy(r => r.CardName),
            "price" => result.OrderByDescending(r => r.LastPrice ?? 0),
            "change" => result.OrderByDescending(r => r.PriceChangePercent24h ?? decimal.MinValue),
            _ => result.OrderBy(r => r.CardName)
        };
    }

    // RN05: variation in coins and in percent relative to the baseline price.
    public static (int? Change, decimal? Percent) ComputeChange(int? lastPrice, int? baselinePrice)
    {
        if (!lastPrice.HasValue || !baselinePrice.HasValue || baselinePrice.Value <= 0)
            return (null, null);

        int change = lastPrice.Value - baselinePrice.Value;
        decimal percent = Math.Round((decimal)change / baselinePrice.Value * 100m, 2);
        return (change, percent);
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

        if (item.BuyTarget != dto.BuyTarget)
            item.BuyAlertTriggered = false;
            
        if (item.SellTarget != dto.SellTarget)
            item.SellAlertTriggered = false;

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