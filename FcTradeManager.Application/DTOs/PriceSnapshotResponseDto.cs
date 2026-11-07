using FcTradeManager.Domain.Enums;

namespace FcTradeManager.Application.DTOs;

public class PriceSnapshotResponseDto
{
    public Guid Id { get; set; }
    public Guid CardId { get; set; }
    public int Price { get; set; }
    public PriceSource Source { get; set; }
    public DateTime RecordedAt { get; set; }
}