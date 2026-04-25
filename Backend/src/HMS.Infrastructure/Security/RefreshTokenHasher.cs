using HMS.Application.Abstractions.Security;
using BCrypt.Net;

namespace HMS.Infrastructure.Security;

public class RefreshTokenHasher : IRefreshTokenHasher
{
    public string Hash(string token)
    {
        return BCrypt.Net.BCrypt.HashPassword(token);
    }

    public bool Verify(string token, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(token, hash);
    }
}