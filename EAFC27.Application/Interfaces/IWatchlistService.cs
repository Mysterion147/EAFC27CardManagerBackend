using EAFC27.Application.DTOs;

namespace EAFC27.Application.Interfaces;

public interface IWatchlistService
{
    Task<IEnumerable<WatchlistItemResponseDto>> GetUserWatchlistAsync(Guid userId);
    Task<WatchlistItemResponseDto> AddToWatchlistAsync(Guid userId, CreateWatchlistDto dto);
    Task<WatchlistItemResponseDto> UpdateWatchlistAsync(Guid userId, Guid id, UpdateWatchlistDto dto);
    Task RemoveFromWatchlistAsync(Guid userId, Guid id);
}