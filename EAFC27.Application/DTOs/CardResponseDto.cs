namespace EAFC27.Application.DTOs;

public class CardResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Overall { get; set; }
    public string Position { get; set; } = string.Empty;
    public string Club { get; set; } = string.Empty;
    public string Rarity { get; set; } = string.Empty;
    public string ExternalRef { get; set; } = string.Empty;
}