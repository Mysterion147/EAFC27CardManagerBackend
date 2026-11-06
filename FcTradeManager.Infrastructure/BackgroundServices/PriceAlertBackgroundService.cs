using FcTradeManager.Domain.Entities;
using FcTradeManager.Domain.Rules;
using FcTradeManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FcTradeManager.Infrastructure.BackgroundServices;

public class PriceAlertBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PriceAlertBackgroundService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(1);

    public PriceAlertBackgroundService(IServiceProvider serviceProvider, ILogger<PriceAlertBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("PriceAlertBackgroundService started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                await EvaluatePricesAndGenerateAlertsAsync(context, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to evaluate prices and generate alerts in background.");
            }

            try
            {
                await Task.Delay(_checkInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    internal async Task EvaluatePricesAndGenerateAlertsAsync(AppDbContext context, CancellationToken ct) // internal so Tests can call it
    {
        // tracked on purpose: we update the BuyAlertTriggered / SellAlertTriggered flags
        var watchlistItems = await context.WatchlistItems.ToListAsync(ct);
        if (watchlistItems.Count == 0) return;

        // latest price per card
        var priceByCard = new Dictionary<Guid, int>();
        foreach (var cardId in watchlistItems.Select(w => w.CardId).Distinct())
        {
            var price = await context.PriceSnapshots
                .Where(p => p.CardId == cardId)
                .OrderByDescending(p => p.RecordedAt)
                .Select(p => (int?)p.Price)
                .FirstOrDefaultAsync(ct);

            if (price.HasValue)
                priceByCard[cardId] = price.Value;
        }

        foreach (var item in watchlistItems)
        {
            if (!priceByCard.TryGetValue(item.CardId, out var price))
                continue;

            var result = AlertRules.Evaluate(
                price,
                item.BuyTarget,
                item.SellTarget,
                item.BuyAlertTriggered,
                item.SellAlertTriggered);

            if (result.FireBuy)
            {
                context.Alerts.Add(NewAlert(item, "buy", price));
                _logger.LogInformation("buy alert generated for user {UserId} on card {CardId} at price {Price}",
                    item.UserId, item.CardId, price);
            }

            if (result.FireSell)
            {
                context.Alerts.Add(NewAlert(item, "sell", price));
                _logger.LogInformation("sell alert generated for user {UserId} on card {CardId} at price {Price}",
                    item.UserId, item.CardId, price);
            }

            item.BuyAlertTriggered = result.BuyTriggered;
            item.SellAlertTriggered = result.SellTriggered;
        }

        await context.SaveChangesAsync(ct);
    }

    private static Alert NewAlert(WatchlistItem item, string kind, int price) => new()
    {
        Id = Guid.NewGuid(),
        UserId = item.UserId,
        CardId = item.CardId,
        Kind = kind,
        TriggeredPrice = price,
        CreatedAt = DateTime.UtcNow
    };
}
