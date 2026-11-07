using System.ComponentModel.DataAnnotations;

namespace FcTradeManager.Application.DTOs;

public class RefreshRequestDto
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}