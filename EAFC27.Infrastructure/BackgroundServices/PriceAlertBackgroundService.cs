using EAFC27.Domain.Entities;
using EAFC27.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EAFC27.Infrastructure.BackgroundServices;

public class PriceAlertBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PriceAlertBackgroundService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(1); // every one minute

    public PriceAlertBackgroundService(IServiceProvider serviceProvider, ILogger<PriceAlertBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("PriceAlertBackgroundService iniciado.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    await EvaluatePricesAndGenerateAlertsAsync(context);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to evaluate prices and generate alerts in background.");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }
    }

    private async Task EvaluatePricesAndGenerateAlertsAsync(AppDbContext context)
    {
        var watchlistItems = await context.WatchlistItems
            .Include(w => w.Card)
            .ToListAsync();

        if (!watchlistItems.Any()) return;

        foreach (var item in watchlistItems)
        {
            var latestSnapshot = await context.PriceSnapshots
                .Where(p => p.CardId == item.CardId)
                .OrderByDescending(p => p.RecordedAt)
                .FirstOrDefaultAsync();

            if (latestSnapshot == null) continue;

            int currentPrice = latestSnapshot.Price;

            if (currentPrice <= item.BuyTarget)
            {
                await CreateAlertIfNotExistsAsync(context, item.UserId, item.CardId, "buy", currentPrice);
            }

            if (currentPrice >= item.SellTarget)
            {
                await CreateAlertIfNotExistsAsync(context, item.UserId, item.CardId, "sell", currentPrice);
            }
        }
    }

    private async Task CreateAlertIfNotExistsAsync(AppDbContext context, Guid userId, Guid cardId, string kind, int currentPrice)
    {
        var existingUnreadAlert = await context.Alerts
            .Where(a => a.UserId == userId && a.CardId == cardId && a.Kind == kind && a.ReadAt == null)
            .FirstOrDefaultAsync();

        if (existingUnreadAlert == null)
        {
            var alert = new Alert
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                CardId = cardId,
                Kind = kind,
                TriggeredPrice = currentPrice,
                CreatedAt = DateTime.UtcNow,
                ReadAt = null
            };

            await context.Alerts.AddAsync(alert);
            await context.SaveChangesAsync();
            _logger.LogInformation("{Kind} alert generated for user {UserId} on card {CardId} at price {Price}", kind, userId, cardId, currentPrice);
        }
    }
}