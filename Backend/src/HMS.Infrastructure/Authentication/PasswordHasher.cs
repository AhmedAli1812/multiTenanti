using HMS.Application.Abstractions.Auth;
using BCrypt.Net;

namespace HMS.Infrastructure.Authentication;

public class PasswordHasher : IPasswordHasher
{
    // ✅ Hash Password
    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    // ✅ Verify Password
    public bool Verify(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }
}