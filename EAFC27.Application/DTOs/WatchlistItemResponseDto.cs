namespace EAFC27.Application.DTOs;

public class WatchlistItemResponseDto
{
    public Guid Id { get; set; }
    public Guid CardId { get; set; }
    public string CardName { get; set; } = string.Empty;
    public int Overall { get; set; }
    public string Position { get; set; } = string.Empty;
    public string Club { get; set; } = string.Empty;
    public string Rarity { get; set; } = string.Empty;
    public int TargetBuyPrice { get; set; }
    public int TargetSellPrice { get; set; }
    public DateTime CreatedAt { get; set; }
}