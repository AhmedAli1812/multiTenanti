using HMS.Domain.Entities.Identity;

namespace HMS.Application.Abstractions.Auth;

public interface IJwtService
{
    // 🔥 Updated
    string GenerateToken(
        Guid userId,
        Guid? tenantId,
        List<string> roles,
        List<string> permissions,
        User user);

    string GenerateRefreshToken();
}