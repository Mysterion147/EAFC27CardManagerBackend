using FcTradeManager.Domain.Enums;
using FcTradeManager.Application.DTOs;
using FcTradeManager.Application.Interfaces;
using FcTradeManager.Domain.Entities;
using FcTradeManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FcTradeManager.Infrastructure.Services;

public class TradeService : ITradeService
{
    private readonly AppDbContext _context;
    private readonly TradeCalculationService _calculationService;

    public TradeService(AppDbContext context, TradeCalculationService calculationService)
    {
        _context = context;
        _calculationService = calculationService;
    }

    public async Task<IEnumerable<TradeResponseDto>> GetUserTradesAsync(Guid userId)
    {
        // Load all trades for the user in one query to avoid multiple database hits
        var allTrades = await _context.Trades
            .Where(t => t.UserId == userId)
            .Include(t => t.Card)
            .OrderByDescending(t => t.ExecutedAt)
            .AsNoTracking()
            .ToListAsync();

        var result = new List<TradeResponseDto>();

        foreach (var trade in allTrades)
        {
            decimal profit = 0;
            if (trade.Type == TradeType.Sell)
            {
                profit = TradeCalculationService.CalculateProfit(allTrades, trade);
            }

            result.Add(new TradeResponseDto
            {
                Id = trade.Id,
                CardId = trade.CardId,
                CardName = trade.Card.Name,
                Type = trade.Type,
                Price = trade.Price,
                Quantity = trade.Quantity,
                CalculatedProfit = profit,
                ExecutedAt = trade.ExecutedAt
            });
        }

        return result;
    }

    public async Task<TradeResponseDto> AddTradeAsync(Guid userId, CreateTradeDto dto)
    {
        var card = await _context.Cards.FindAsync(dto.CardId);
        if (card == null)
            throw new KeyNotFoundException("Card not found in the catalog.");

        var trade = new Trade
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CardId = dto.CardId,
            Type = dto.Type,
            Price = dto.Price,
            Quantity = dto.Quantity,
            ExecutedAt = DateTime.UtcNow
        };

        // validate before saving
        await _calculationService.ValidateAfterChangeAsync(
            userId, new[] { dto.CardId }, removed: null, added: trade);

        await _context.Trades.AddAsync(trade);
        await _context.SaveChangesAsync();

        var allTrades = await _context.Trades.Where(t => t.UserId == userId).ToListAsync();
        var profit = TradeCalculationService.CalculateProfit(allTrades, trade);

        return new TradeResponseDto
        {
            Id = trade.Id,
            CardId = card.Id,
            CardName = card.Name,
            Type = trade.Type,
            Price = trade.Price,
            Quantity = trade.Quantity,
            CalculatedProfit = profit,
            ExecutedAt = trade.ExecutedAt
        };
    }

    public async Task<TradeResponseDto> UpdateTradeAsync(Guid userId, Guid tradeId, CreateTradeDto dto)
    {
        var trade = await _context.Trades
            .FirstOrDefaultAsync(t => t.Id == tradeId && t.UserId == userId);

        if (trade == null)
            throw new KeyNotFoundException("Trade not found.");

        var card = await _context.Cards.FindAsync(dto.CardId);
        if (card == null)
            throw new KeyNotFoundException("Card not found in the catalog.");

        // copy with the new values
        var updated = new Trade
        {
            Id = trade.Id,
            UserId = userId,
            CardId = dto.CardId,
            Type = dto.Type,
            Price = dto.Price,
            Quantity = dto.Quantity,
            ExecutedAt = trade.ExecutedAt
        };

        // validates the OLD card and the NEW card
        await _calculationService.ValidateAfterChangeAsync(
            userId, new[] { trade.CardId, dto.CardId }, removed: trade, added: updated);

        trade.CardId = dto.CardId;
        trade.Type = dto.Type;
        trade.Price = dto.Price;
        trade.Quantity = dto.Quantity;

        await _context.SaveChangesAsync();

        var allTrades = await _context.Trades.Where(t => t.UserId == userId).ToListAsync();
        var profit = TradeCalculationService.CalculateProfit(allTrades, trade);

        return new TradeResponseDto
        {
            Id = trade.Id,
            CardId = card.Id,
            CardName = card.Name,
            Type = trade.Type,
            Price = trade.Price,
            Quantity = trade.Quantity,
            CalculatedProfit = profit,
            ExecutedAt = trade.ExecutedAt
        };
    }

    public async Task DeleteTradeAsync(Guid userId, Guid tradeId)
    {
        var trade = await _context.Trades
            .FirstOrDefaultAsync(t => t.Id == tradeId && t.UserId == userId);

        if (trade == null)
            throw new KeyNotFoundException("Trade not found.");

        await _calculationService.ValidateAfterChangeAsync(
            userId, new[] { trade.CardId }, removed: trade, added: null);

        _context.Trades.Remove(trade);
        await _context.SaveChangesAsync();
    }


    public async Task<DashboardResponseDto> GetDashboardAsync(Guid userId)
    {
        var tradesResponse = await GetUserTradesAsync(userId);
        var sellTrades = tradesResponse.Where(t => t.Type == TradeType.Sell).ToList();

        decimal totalProfit = sellTrades.Sum(t => t.CalculatedProfit);
        decimal? bestTrade = sellTrades.Any() ? sellTrades.Max(t => (decimal?)t.CalculatedProfit) : null;
        decimal? worstTrade = sellTrades.Any() ? sellTrades.Min(t => (decimal?)t.CalculatedProfit) : null;

        var allRawTrades = await _context.Trades
            .Where(t => t.UserId == userId)
            .Include(t => t.Card)
            .ToListAsync();

        var openPositions = allRawTrades
            .GroupBy(t => new { t.CardId, t.Card.Name })
            .Select(g => new OpenPositionDto
            {
                CardId = g.Key.CardId,
                CardName = g.Key.Name,
                Quantity = g.Where(t => t.Type == TradeType.Buy).Sum(t => t.Quantity) - g.Where(t => t.Type == TradeType.Sell).Sum(t => t.Quantity)
            })
            .Where(p => p.Quantity > 0)
            .ToList();

        return new DashboardResponseDto
        {
            TotalProfit = totalProfit,
            BestTradeProfit = bestTrade,
            WorstTradeProfit = worstTrade,
            OpenPositionsCount = openPositions.Count,
            OpenPositions = openPositions
        };
    }
}