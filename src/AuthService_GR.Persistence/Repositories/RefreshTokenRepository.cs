using AuthService_GR.Domain.Entities;
using AuthService_GR.Domain.Interfaces;
using AuthService_GR.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace AuthService_GR.Persistence.Repositories;

public class RefreshTokenRepository(ApplicationDbContext context) : IRefreshTokenRepository
{
    public async Task<RefreshToken> CreateAsync(RefreshToken refreshToken)
    {
        context.RefreshTokens.Add(refreshToken);
        await context.SaveChangesAsync();
        return refreshToken;
    }

    public async Task<RefreshToken?> GetByTokenAsync(string token)
    {
        return await context.RefreshTokens
            .Include(rt => rt.User)
                .ThenInclude(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
            .Include(rt => rt.User)
                .ThenInclude(u => u.UserProfile)
            .FirstOrDefaultAsync(rt => rt.Token == token);
    }

    public async Task<RefreshToken> UpdateAsync(RefreshToken refreshToken)
    {
        await context.SaveChangesAsync();
        return refreshToken;
    }

    public async Task RevokeAllForUserAsync(string userId)
    {
        var activeTokens = await context.RefreshTokens
            .Where(rt => rt.UserId == userId && rt.RevokedAt == null)
            .ToListAsync();

        foreach (var token in activeTokens)
        {
            token.RevokedAt = DateTime.UtcNow;
        }

        await context.SaveChangesAsync();
    }
}
