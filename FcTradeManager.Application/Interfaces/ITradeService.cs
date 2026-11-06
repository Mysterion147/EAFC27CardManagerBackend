using FcTradeManager.Application.DTOs;

namespace FcTradeManager.Application.Interfaces;

public interface ITradeService
{
    Task<IEnumerable<TradeResponseDto>> GetUserTradesAsync(Guid userId);
    Task<TradeResponseDto> AddTradeAsync(Guid userId, CreateTradeDto dto);
    Task<TradeResponseDto> UpdateTradeAsync(Guid userId, Guid tradeId, CreateTradeDto dto);
    Task DeleteTradeAsync(Guid userId, Guid tradeId);
    Task<DashboardResponseDto> GetDashboardAsync(Guid userId);
}