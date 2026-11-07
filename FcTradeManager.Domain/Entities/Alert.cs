using FcTradeManager.Domain.Enums;

namespace FcTradeManager.Domain.Entities;

public class Alert
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    
    public Guid CardId { get; set; }
    public Card Card { get; set; } = null!;
    
    public AlertKind Kind { get; set; }
    public int TriggeredPrice { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReadAt { get; set; }
}