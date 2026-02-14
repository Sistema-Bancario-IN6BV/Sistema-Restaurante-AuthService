using System.Security.Cryptography;

namespace AuthServiceIN6BV.Application.Services;

public static class TokenGeneratorService
{
    public static string GenerateEmailVerificationToken()
    {
        return GenerateSecureToken(32);

    }

    public static string GeneratePasswordResetToken()
    {
        return GenerateSecureToken(32);
    }

    public static string GenerateSecureToken(int lenght)
    {
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[lenght];
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes).Replace("+","-").Replace("/","_").Replace("=","");    
    }
}