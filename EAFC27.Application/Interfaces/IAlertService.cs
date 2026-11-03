using EAFC27.Application.DTOs;

namespace EAFC27.Application.Interfaces;

public interface IAlertService
{
    Task<IEnumerable<AlertResponseDto>> GetUserAlertsAsync(Guid userId);
    Task MarkAlertAsReadAsync(Guid userId, Guid alertId);
}