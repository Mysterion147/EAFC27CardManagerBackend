namespace EAFC27.Domain.Entities;

public class Alert
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    
    public Guid CardId { get; set; }
    public Card Card { get; set; } = null!;
    
    public string Kind { get; set; } = string.Empty; // "buy" ou "sell"
    public int TriggeredPrice { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReadAt { get; set; }
}