using FcTradeManager.Application.DTOs;

namespace FcTradeManager.Application.Interfaces;

public interface IWatchlistService
{
    Task<IEnumerable<WatchlistItemResponseDto>> GetUserWatchlistAsync(Guid userId, string? sortBy = null);
    Task<WatchlistItemResponseDto> AddToWatchlistAsync(Guid userId, CreateWatchlistDto dto);
    Task<WatchlistItemResponseDto> UpdateWatchlistAsync(Guid userId, Guid id, UpdateWatchlistDto dto);
    Task RemoveFromWatchlistAsync(Guid userId, Guid id);
}