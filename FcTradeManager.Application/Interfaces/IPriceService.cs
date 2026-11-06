using FcTradeManager.Application.DTOs;

namespace FcTradeManager.Application.Interfaces;

public interface IPriceService
{
    Task<PriceSnapshotResponseDto> AddPriceSnapshotAsync(Guid cardId, CreatePriceSnapshotDto dto, string source = "manual");
    Task<IEnumerable<PriceSnapshotResponseDto>> GetPriceHistoryAsync(Guid cardId, string period = "24h");
}