namespace EAFC27.Domain.Entities;

public class Card
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Overall { get; set; }
    public string Position { get; set; } = string.Empty;
    public string Club { get; set; } = string.Empty;
    public string Rarity { get; set; } = string.Empty;
    public string ExternalRef { get; set; } = string.Empty;
    
    // Relacionamentos da entidade conforme a documentação
    public ICollection<PriceSnapshot> PriceSnapshots { get; set; } = new List<PriceSnapshot>();
    public ICollection<WatchlistItem> WatchlistItems { get; set; } = new List<WatchlistItem>();
    public ICollection<Trade> Trades { get; set; } = new List<Trade>();
}