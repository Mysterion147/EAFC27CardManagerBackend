using FcTradeManager.Application.DTOs;

namespace FcTradeManager.Application.Interfaces;

public interface IAlertService
{
    Task<IEnumerable<AlertResponseDto>> GetUserAlertsAsync(Guid userId);
    Task MarkAlertAsReadAsync(Guid userId, Guid alertId);
}