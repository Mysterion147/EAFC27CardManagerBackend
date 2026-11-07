using System.ComponentModel.DataAnnotations;
using EAFC27.Domain.Enums;

namespace FcTradeManager.Application.DTOs;

public class CreateTradeDto
{
    [Required]
    public Guid CardId { get; set; }

    [Required]
    [EnumDataType(typeof(TradeType))]
    public TradeType Type { get; set; }

    [Range(200, 15000000, ErrorMessage = "The price must be between 200 and 15,000,000.")]
    public int Price { get; set; }

    [Range(1, 100, ErrorMessage = "Quantity must be between 1 and 100.")]
    public int Quantity { get; set; }
}