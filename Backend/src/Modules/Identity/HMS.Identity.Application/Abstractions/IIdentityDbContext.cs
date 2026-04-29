using HMS.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HMS.Identity.Application.Abstractions;

/// <summary>
/// Identity module's slice of the shared DbContext.
/// Handlers depend on this interface — never on the concrete ApplicationDbContext.
/// This enforces module isolation: Identity cannot accidentally query Patients tables.
/// </summary>
public interface IIdentityDbContext
{
    DbSet<User>           Users           { get; }
    DbSet<Role>           Roles           { get; }
    DbSet<Permission>     Permissions     { get; }
    DbSet<UserRole>       UserRoles       { get; }
    DbSet<RolePermission> RolePermissions { get; }
    DbSet<RefreshToken>   RefreshTokens   { get; }
    DbSet<UserSession>    UserSessions    { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Password hashing abstraction — keeps BCrypt out of Application layer.
/// </summary>
public interface IPasswordHasher
{
    string Hash(string password);
    bool   Verify(string password, string hash);
}

/// <summary>
/// JWT service abstraction — keeps JWT internals out of Application layer.
/// </summary>
public interface IJwtService
{
    string GenerateAccessToken(User user, List<string> roles, List<string> permissions);
    string GenerateRefreshToken();
}
