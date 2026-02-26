using System.ComponentModel.DataAnnotations;

namespace AuthService_GR.Application.DTOs.Email;

public class ResendVerificationDto
{
    [Required]
    [EmailAddress]
    public string Email {get; set; } = string.Empty;
}