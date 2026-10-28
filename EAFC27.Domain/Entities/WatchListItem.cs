namespace EAFC27.Domain.Entities;

public class WatchlistItem
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    
    public Guid CardId { get; set; }
    public Card Card { get; set; } = null!;
    
    public int BuyTarget { get; set; }
    public int SellTarget { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}