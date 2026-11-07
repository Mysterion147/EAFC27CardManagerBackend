using FcTradeManager.Domain.Enums;

namespace FcTradeManager.Application.DTOs;

public class AlertResponseDto
{
    public Guid Id { get; set; }
    public Guid CardId { get; set; }
    public string CardName { get; set; } = string.Empty;
    public AlertKind Kind { get; set; }
    public int TriggeredPrice { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReadAt { get; set; }
}