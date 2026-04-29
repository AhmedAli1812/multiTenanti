using HMS.Domain.Entities.Identity; 

namespace HMS.Application.Abstractions.Auth;

public interface IJwtService
{
    // 🔥 Updated
    public string GenerateToken(
     Guid userId,
     Guid? tenantId,
     string? tenantName,
     List<string>? roles,
     List<string>? permissions,
     Guid? branchId,
     string? branchName,
     User user
    );

    public string GenerateRefreshToken();
}