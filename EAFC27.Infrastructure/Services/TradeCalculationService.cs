using EAFC27.Domain.Entities;
using EAFC27.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EAFC27.Infrastructure.Services;

public class TradeCalculationService
{
    private readonly AppDbContext _context;

    public TradeCalculationService(AppDbContext context)
    {
        _context = context;
    }

    public decimal CalculateProfit(IEnumerable<Trade> historicalTrades, Trade targetSellTrade)
    {
        if (targetSellTrade.Type != "sell") return 0;

        var buyTrades = historicalTrades
            .Where(t => t.CardId == targetSellTrade.CardId && t.Type == "buy" && t.ExecutedAt <= targetSellTrade.ExecutedAt && t.Id != targetSellTrade.Id)
            .ToList();

        var totalBuyQty = buyTrades.Sum(b => b.Quantity);
        var totalBuyCost = buyTrades.Sum(b => b.Price * b.Quantity);
        decimal avgBuyPrice = totalBuyQty > 0 ? (decimal)totalBuyCost / totalBuyQty : 0;

        // RN01 & RN02: Preço de venda líquido (preço * 0.95) menos custo médio de compra
        decimal netSellPrice = targetSellTrade.Price * 0.95m;
        return Math.Round((netSellPrice - avgBuyPrice) * targetSellTrade.Quantity, 2);
    }

    public async Task ValidateOpenPositionAsync(Guid userId, Guid cardId, string newType, int newQuantity, Guid? excludeTradeId = null)
    {
        var trades = await _context.Trades
            .Where(t => t.UserId == userId && t.CardId == cardId)
            .ToListAsync();

        if (excludeTradeId.HasValue)
        {
            trades = trades.Where(t => t.Id != excludeTradeId.Value).ToList();
        }

        int totalBought = trades.Where(t => t.Type == "buy").Sum(t => t.Quantity);
        int totalSold = trades.Where(t => t.Type == "sell").Sum(t => t.Quantity);

        if (newType == "buy") totalBought += newQuantity;
        if (newType == "sell") totalSold += newQuantity;

        // RN04: Posição aberta nunca pode ser negativa
        if (totalBought - totalSold < 0)
        {
            throw new InvalidOperationException("Operation violates the open position rule. The quantity sold would exceed the total purchased.");
        }
    }

    public async Task ValidateDeleteAsync(Guid userId, Guid tradeId)
    {
        var trade = await _context.Trades.FirstOrDefaultAsync(t => t.Id == tradeId && t.UserId == userId);
        if (trade == null || trade.Type != "buy") return;

        var cardTrades = await _context.Trades
            .Where(t => t.UserId == userId && t.CardId == trade.CardId && t.Id != tradeId)
            .OrderBy(t => t.ExecutedAt)
            .ToListAsync();

        int runningBalance = 0;
        foreach (var t in cardTrades)
        {
            if (t.Type == "buy") runningBalance += t.Quantity;
            else runningBalance -= t.Quantity;

            if (runningBalance < 0)
            {
                throw new InvalidOperationException("It's not possible to delete this purchase as it would result in a negative open position in subsequent transactions.");
            }
        }
    }
}