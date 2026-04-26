using HMS.Domain.Entities.Identity;

namespace HMS.Application.Abstractions.Auth;

public interface IJwtService
{
    // 🔥 Updated
    public string GenerateToken(
     Guid userId,
     Guid? tenantId,
     List<string>? roles,
     List<string>? permissions,
     Guid? branchId, // 🔥 قبل user
     User user
    );

    public string GenerateRefreshToken();
}