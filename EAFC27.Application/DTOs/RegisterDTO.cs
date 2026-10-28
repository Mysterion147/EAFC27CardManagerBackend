using System.ComponentModel.DataAnnotations;

namespace EAFC27.Application.DTOs;

public class RegisterDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(8, ErrorMessage = "A senha deve ter no mínimo 8 caracteres.")]
    public string Password { get; set; } = string.Empty;
}