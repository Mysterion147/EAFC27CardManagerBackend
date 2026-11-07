using EAFC27.Domain.Enums;

namespace FcTradeManager.Domain.Entities;

public class PriceSnapshot
{
    public Guid Id { get; set; }
    public Guid CardId { get; set; }
    public Card Card { get; set; } = null!;
    
    public int Price { get; set; }
    public PriceSource Source { get; set; } = PriceSource.Manual;
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
}