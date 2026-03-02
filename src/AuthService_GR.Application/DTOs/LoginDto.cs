using System.ComponentModel.DataAnnotations;

namespace AuthService_GR.Application.DTOs;

public class LoginDto
{
    [Required]
    public string EmailOrUsername { get; set;} = string.Empty;

    [Required]
    public string Password {get; set;} = string.Empty;
}