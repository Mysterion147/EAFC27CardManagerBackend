using EAFC27.Application.DTOs;
using EAFC27.Application.Interfaces;
using EAFC27.Domain.Entities;
using EAFC27.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EAFC27.Infrastructure.Services;

public class TradeService : ITradeService
{
    private readonly AppDbContext _context;

    public TradeService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<TradeResponseDto>> GetUserTradesAsync(Guid userId)
    {
        var trades = await _context.Trades
            .Where(t => t.UserId == userId)
            .Include(t => t.Card)
            .OrderByDescending(t => t.ExecutedAt)
            .AsNoTracking()
            .ToListAsync();

        var result = new List<TradeResponseDto>();

        foreach (var trade in trades)
        {
            decimal profit = 0;
            if (trade.Type == "sell")
            {
                var buyTrades = await _context.Trades
                    .Where(t => t.UserId == userId && t.CardId == trade.CardId && t.Type == "buy" && t.ExecutedAt <= trade.ExecutedAt)
                    .ToListAsync();

                var totalBuyQty = buyTrades.Sum(b => b.Quantity);
                var totalBuyCost = buyTrades.Sum(b => b.Price * b.Quantity);
                decimal avgBuyPrice = totalBuyQty > 0 ? (decimal)totalBuyCost / totalBuyQty : 0;

                decimal netSellPrice = trade.Price * 0.95m;
                profit = (netSellPrice - avgBuyPrice) * trade.Quantity;
            }

            result.Add(new TradeResponseDto
            {
                Id = trade.Id,
                CardId = trade.CardId,
                CardName = trade.Card.Name,
                Type = trade.Type,
                Price = trade.Price,
                Quantity = trade.Quantity,
                CalculatedProfit = Math.Round(profit, 2),
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

        if (dto.Type == "sell")
        {
            var userTrades = await _context.Trades
                .Where(t => t.UserId == userId && t.CardId == dto.CardId)
                .ToListAsync();

            int totalBought = userTrades.Where(t => t.Type == "buy").Sum(t => t.Quantity);
            int totalSold = userTrades.Where(t => t.Type == "sell").Sum(t => t.Quantity);
            int openPosition = totalBought - totalSold;

            if (dto.Quantity > openPosition)
                throw new InvalidOperationException($"Exceeds available position. Available: {openPosition} units.");
        }

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

        await _context.Trades.AddAsync(trade);
        await _context.SaveChangesAsync();

        decimal profit = 0;
        if (trade.Type == "sell")
        {
            var buyTrades = await _context.Trades
                .Where(t => t.UserId == userId && t.CardId == trade.CardId && t.Type == "buy" && t.Id != trade.Id)
                .ToListAsync();

            var totalBuyQty = buyTrades.Sum(b => b.Quantity);
            var totalBuyCost = buyTrades.Sum(b => b.Price * b.Quantity);
            decimal avgBuyPrice = totalBuyQty > 0 ? (decimal)totalBuyCost / totalBuyQty : 0;

            decimal netSellPrice = trade.Price * 0.95m;
            profit = (netSellPrice - avgBuyPrice) * trade.Quantity;
        }

        return new TradeResponseDto
        {
            Id = trade.Id,
            CardId = card.Id,
            CardName = card.Name,
            Type = trade.Type,
            Price = trade.Price,
            Quantity = trade.Quantity,
            CalculatedProfit = Math.Round(profit, 2),
            ExecutedAt = trade.ExecutedAt
        };
    }

    public async Task<TradeResponseDto> UpdateTradeAsync(Guid userId, Guid tradeId, CreateTradeDto dto)
    {
        var trade = await _context.Trades
            .Include(t => t.Card)
            .FirstOrDefaultAsync(t => t.Id == tradeId && t.UserId == userId);

        if (trade == null)
            throw new KeyNotFoundException("Trade not found.");

        trade.CardId = dto.CardId;
        trade.Type = dto.Type;
        trade.Price = dto.Price;
        trade.Quantity = dto.Quantity;

        await _context.SaveChangesAsync();

        return new TradeResponseDto
        {
            Id = trade.Id,
            CardId = trade.CardId,
            CardName = trade.Card.Name,
            Type = trade.Type,
            Price = trade.Price,
            Quantity = trade.Quantity,
            CalculatedProfit = 0,
            ExecutedAt = trade.ExecutedAt
        };
    }

    public async Task DeleteTradeAsync(Guid userId, Guid tradeId)
    {
        var trade = await _context.Trades
            .FirstOrDefaultAsync(t => t.Id == tradeId && t.UserId == userId);

        if (trade == null)
            throw new KeyNotFoundException("Trade not found.");

        _context.Trades.Remove(trade);
        await _context.SaveChangesAsync();
    }

    public async Task<DashboardResponseDto> GetDashboardAsync(Guid userId)
    {
        var trades = await GetUserTradesAsync(userId);
        
        var sellTrades = trades.Where(t => t.Type == "sell").ToList();
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
                Quantity = g.Where(t => t.Type == "buy").Sum(t => t.Quantity) - g.Where(t => t.Type == "sell").Sum(t => t.Quantity)
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