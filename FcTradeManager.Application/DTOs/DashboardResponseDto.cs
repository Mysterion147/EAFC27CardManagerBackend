namespace FcTradeManager.Application.DTOs;

public class DashboardResponseDto
{
    public decimal TotalProfit { get; set; }
    public decimal? BestTradeProfit { get; set; }
    public decimal? WorstTradeProfit { get; set; }
    public int OpenPositionsCount { get; set; }
    public IEnumerable<OpenPositionDto> OpenPositions { get; set; } = new List<OpenPositionDto>();
}

public class OpenPositionDto
{
    public Guid CardId { get; set; }
    public string CardName { get; set; } = string.Empty;
    public int Quantity { get; set; }
}