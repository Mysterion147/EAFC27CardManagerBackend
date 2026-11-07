using EAFC27.Domain.Enums;
using FcTradeManager.Domain.Entities;
using FcTradeManager.Infrastructure.Data;

namespace FcTradeManager.Tests.Support;

public static class Seed
{
    public static User AddUser(AppDbContext db, string email = "user@test.com")
    {
        var user = new User { Id = Guid.NewGuid(), Email = email, PasswordHash = "hash" };
        db.Users.Add(user);
        db.SaveChanges();
        return user;
    }

    public static Card AddCard(AppDbContext db, string name = "Test Card")
    {
        var card = new Card
        {
            Id = Guid.NewGuid(),
            Name = name,
            Overall = 90,
            Position = "ST",
            Club = "Test FC",
            Rarity = "Gold Rare",
            ExternalRef = $"ref-{Guid.NewGuid():N}"
        };
        db.Cards.Add(card);
        db.SaveChanges();
        return card;
    }

    public static Trade AddTrade(AppDbContext db, Guid userId, Guid cardId,
        TradeType type, int price, int quantity, DateTime executedAt)
    {
        var trade = new Trade
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CardId = cardId,
            Type = type,
            Price = price,
            Quantity = quantity,
            ExecutedAt = executedAt
        };
        db.Trades.Add(trade);
        db.SaveChanges();
        return trade;
    }

    public static PriceSnapshot AddSnapshot(AppDbContext db, Guid cardId,
        int price, DateTime recordedAt, PriceSource source = PriceSource.Manual)
    {
        var snapshot = new PriceSnapshot
        {
            Id = Guid.NewGuid(),
            CardId = cardId,
            Price = price,
            Source = source,
            RecordedAt = recordedAt
        };
        db.PriceSnapshots.Add(snapshot);
        db.SaveChanges();
        return snapshot;
    }

    public static WatchlistItem AddWatch(AppDbContext db, Guid userId, Guid cardId,
        int buyTarget, int sellTarget, bool buyTriggered = false, bool sellTriggered = false)
    {
        var item = new WatchlistItem
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CardId = cardId,
            BuyTarget = buyTarget,
            SellTarget = sellTarget,
            BuyAlertTriggered = buyTriggered,
            SellAlertTriggered = sellTriggered
        };
        db.WatchlistItems.Add(item);
        db.SaveChanges();
        return item;
    }

    public static Alert AddAlert(AppDbContext db, Guid userId, Guid cardId,
        AlertKind kind, int price, DateTime createdAt, DateTime? readAt = null)
    {
        var alert = new Alert
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CardId = cardId,
            Kind = kind,
            TriggeredPrice = price,
            CreatedAt = createdAt,
            ReadAt = readAt
        };
        db.Alerts.Add(alert);
        db.SaveChanges();
        return alert;
    }
}
