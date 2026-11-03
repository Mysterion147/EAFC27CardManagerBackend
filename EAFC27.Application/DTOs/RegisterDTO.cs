using System.ComponentModel.DataAnnotations;

namespace EAFC27.Application.DTOs;

public class RegisterDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(8, ErrorMessage = "The password must be at least 8 characters long.")]
    public string Password { get; set; } = string.Empty;
}