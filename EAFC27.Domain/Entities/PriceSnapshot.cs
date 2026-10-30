namespace EAFC27.Domain.Entities;

public class PriceSnapshot
{
    public Guid Id { get; set; }
    public Guid CardId { get; set; }
    public Card Card { get; set; } = null!;
    
    public int Price { get; set; }
    public string Source { get; set; } = "manual"; // "manual" or "simulated"
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
}