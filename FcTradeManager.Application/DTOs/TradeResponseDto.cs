using EAFC27.Domain.Enums;

namespace FcTradeManager.Application.DTOs;

public class TradeResponseDto
{
    public Guid Id { get; set; }
    public Guid CardId { get; set; }
    public string CardName { get; set; } = string.Empty;
    public TradeType Type { get; set; }
    public int Price { get; set; }
    public int Quantity { get; set; }
    public decimal CalculatedProfit { get; set; }
    public DateTime ExecutedAt { get; set; }
}