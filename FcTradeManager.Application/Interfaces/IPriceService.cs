using EAFC27.Domain.Enums;
using FcTradeManager.Application.DTOs;

namespace FcTradeManager.Application.Interfaces;

public interface IPriceService
{
    Task<PriceSnapshotResponseDto> AddPriceSnapshotAsync(Guid cardId, CreatePriceSnapshotDto dto, PriceSource source = PriceSource.Manual);
    Task<IEnumerable<PriceSnapshotResponseDto>> GetPriceHistoryAsync(Guid cardId, string period = "24h");
}