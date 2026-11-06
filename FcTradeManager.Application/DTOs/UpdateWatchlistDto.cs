using System.ComponentModel.DataAnnotations;

namespace FcTradeManager.Application.DTOs;

public class UpdateWatchlistDto
{
    [Range(200, 15000000)]
    public int BuyTarget { get; set; }

    [Range(200, 15000000)]
    public int SellTarget { get; set; }
}