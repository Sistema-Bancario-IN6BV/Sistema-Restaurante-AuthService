using AuthService_GR.Application.DTOs;
using AuthService_GR.Application.DTOs.Email;

namespace AuthService_GR.Application.Interfaces;

public interface IAuthService
{
    Task<RegisterResponseDto> RegisterAsync(RegisterDto registerDto, string? frontendBaseUrl = null);
    Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
    Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto refreshTokenDto);
    Task RevokeRefreshTokenAsync(string refreshToken);
    Task<EmailResponseDto> VerifyEmailAsync(VerifyEmailDto verifyEmailDto);
    Task<EmailResponseDto> ResendVerificationEmailAsync(ResendVerificationDto resendDto, string? frontendBaseUrl = null);
    Task<EmailResponseDto> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto, string? frontendBaseUrl = null);
    Task<EmailResponseDto> ResetPasswordAsync(ResetPasswordDto resetPasswordDto);
    Task<UserResponseDto?> GetUserByIdAsync(string userId);
    Task<UserResponseDto?> UpdateProfileAsync(string userId, UpdateProfileDto dto);
    Task<bool> DeactivateAccountAsync(string userId);
}