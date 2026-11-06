using System.ComponentModel.DataAnnotations;

namespace FcTradeManager.Application.DTOs;

public class CreateWatchlistDto
{
    [Required]
    public Guid CardId { get; set; }

    [Range(200, 15000000)]
    public int TargetBuyPrice { get; set; }

    [Range(200, 15000000)]
    public int TargetSellPrice { get; set; }
}