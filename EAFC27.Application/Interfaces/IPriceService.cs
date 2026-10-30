using EAFC27.Application.DTOs;

namespace EAFC27.Application.Interfaces;

public interface IPriceService
{
    Task<PriceSnapshotResponseDto> AddPriceSnapshotAsync(Guid cardId, CreatePriceSnapshotDto dto, string source = "manual");
    Task<IEnumerable<PriceSnapshotResponseDto>> GetPriceHistoryAsync(Guid cardId, string period = "24h");
}