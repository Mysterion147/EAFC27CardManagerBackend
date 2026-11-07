using FcTradeManager.Domain.Enums;
using FcTradeManager.Domain.Entities;
using FcTradeManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FcTradeManager.Infrastructure.Services;

public class TradeCalculationService
{
    private readonly AppDbContext _context;

    public TradeCalculationService(AppDbContext context)
    {
        _context = context;
    }

    // RN01 e RN02: pure calculation, no database access.
    public static decimal CalculateProfit(IEnumerable<Trade> historicalTrades, Trade targetSellTrade)
    {
        if (targetSellTrade.Type != TradeType.Sell) return 0;

        var buyTrades = historicalTrades
            .Where(t => t.CardId == targetSellTrade.CardId
                        && t.Type == TradeType.Buy
                        && t.ExecutedAt <= targetSellTrade.ExecutedAt
                        && t.Id != targetSellTrade.Id)
            .ToList();

        var totalBuyQty = buyTrades.Sum(b => b.Quantity);
        var totalBuyCost = buyTrades.Sum(b => b.Price * b.Quantity);
        decimal avgBuyPrice = totalBuyQty > 0 ? (decimal)totalBuyCost / totalBuyQty : 0;

        // net sell price = price * 0.95 (5% market fee)
        decimal netSellPrice = targetSellTrade.Price * 0.95m;
        return Math.Round((netSellPrice - avgBuyPrice) * targetSellTrade.Quantity, 2);
    }

    // RN04: the open position of a card can never be negative at ANY point in time.
    public static void EnsureNonNegativeBalance(IEnumerable<Trade> trades)
    {
        var balance = 0;

        foreach (var t in trades
                     .OrderBy(t => t.ExecutedAt)
                     .ThenBy(t => t.Type == TradeType.Buy ? 0 : 1)) // same timestamp: buys first
        {
            balance += t.Type == TradeType.Buy ? t.Quantity : -t.Quantity;

            if (balance < 0)
                throw new InvalidOperationException(
                    "Operation would result in a negative open position (sold quantity would exceed purchased quantity).");
        }
    }

    // Simulates the state AFTER a change (add / update / delete) for each affected card
    // and validates it before anything is saved.
    //   add:    removed = null,  added = new trade
    //   update: removed = old,   added = updated copy (may have a different CardId)
    //   delete: removed = trade, added = null
    public async Task ValidateAfterChangeAsync(
        Guid userId,
        IEnumerable<Guid> affectedCardIds,
        Trade? removed,
        Trade? added)
    {
        foreach (var cardId in affectedCardIds.Distinct())
        {
            var trades = await _context.Trades
                .AsNoTracking()
                .Where(t => t.UserId == userId && t.CardId == cardId)
                .ToListAsync();

            if (removed != null)
                trades.RemoveAll(t => t.Id == removed.Id);

            if (added != null && added.CardId == cardId)
                trades.Add(added);

            EnsureNonNegativeBalance(trades);
        }
    }
}
