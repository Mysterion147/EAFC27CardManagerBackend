namespace FcTradeManager.Application.DTOs;

public class AlertResponseDto
{
    public Guid Id { get; set; }
    public Guid CardId { get; set; }
    public string CardName { get; set; } = string.Empty;
    public string Kind { get; set; } = string.Empty; // "buy" ou "sell"
    public int TriggeredPrice { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReadAt { get; set; }
}