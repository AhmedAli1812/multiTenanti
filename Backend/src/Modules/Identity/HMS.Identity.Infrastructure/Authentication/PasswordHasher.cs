using HMS.Identity.Application.Abstractions;

namespace HMS.Identity.Infrastructure.Authentication;

/// <summary>
/// BCrypt-based password hasher.
/// Work factor 12 — strong enough for production, reasonable for login latency.
/// </summary>
public sealed class PasswordHasher : IPasswordHasher
{
    private const int WorkFactor = 12;

    public string Hash(string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);
        return BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
    }

    public bool Verify(string password, string hash)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(hash))
            return false;

        try { return BCrypt.Net.BCrypt.Verify(password, hash); }
        catch { return false; }
    }
}
