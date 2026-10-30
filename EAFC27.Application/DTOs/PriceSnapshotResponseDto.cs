namespace EAFC27.Application.DTOs;

public class PriceSnapshotResponseDto
{
    public Guid Id { get; set; }
    public Guid CardId { get; set; }
    public int Price { get; set; }
    public string Source { get; set; } = string.Empty;
    public DateTime RecordedAt { get; set; }
}