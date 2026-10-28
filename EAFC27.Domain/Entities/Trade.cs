namespace EAFC27.Domain.Entities;

public class Trade
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    
    public Guid CardId { get; set; }
    public Card Card { get; set; } = null!;
    
    public string Type { get; set; } = string.Empty; // "buy" ou "sell"
    public int Price { get; set; }
    public int Quantity { get; set; }
    public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;
}