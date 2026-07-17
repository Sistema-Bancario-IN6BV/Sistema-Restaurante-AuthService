using AuthService_GR.Domain.Entities;

namespace AuthService_GR.Domain.Interfaces;

public interface IRefreshTokenRepository
{
    Task<RefreshToken> CreateAsync(RefreshToken refreshToken);
    Task<RefreshToken?> GetByTokenAsync(string token);
    Task<RefreshToken> UpdateAsync(RefreshToken refreshToken);
    Task RevokeAllForUserAsync(string userId);
}
