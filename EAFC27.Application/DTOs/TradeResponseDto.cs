namespace EAFC27.Application.DTOs;

public class TradeResponseDto
{
    public Guid Id { get; set; }
    public Guid CardId { get; set; }
    public string CardName { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int Price { get; set; }
    public int Quantity { get; set; }
    public decimal CalculatedProfit { get; set; } // estimated profit based on the current market price and the trade price
    public DateTime ExecutedAt { get; set; }
}