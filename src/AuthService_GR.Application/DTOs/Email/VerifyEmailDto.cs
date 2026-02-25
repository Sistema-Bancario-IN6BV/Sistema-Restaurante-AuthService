using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace AuthService_GR.Application.DTOs.Email;

public class VerifyEmailDto
{
    public string Token {get; set;} = string.Empty;
}