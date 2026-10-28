using System.ComponentModel.DataAnnotations;

namespace EAFC27.Application.DTOs;

public class CreateWatchlistDto
{
    [Required]
    public Guid CardId { get; set; }

    [Range(0, 15000000)]
    public int TargetBuyPrice { get; set; }

    [Range(0, 15000000)]
    public int TargetSellPrice { get; set; }
}