using AuthService_GR.Domain.Entities;

namespace AuthService_GR.Application.Interfaces;

public interface IJwtTokenService
{
    string GenerateToken (User user);
}