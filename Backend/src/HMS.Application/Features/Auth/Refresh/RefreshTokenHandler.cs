using HMS.Application.Abstractions.Auth;
using HMS.Application.Abstractions.Persistence;
using HMS.Application.Abstractions.Security;
using HMS.Application.Features.Auth.Login;
using HMS.Domain.Entities.Identity;
using HMS.Shared.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HMS.Application.Features.Auth.Refresh;

public class RefreshTokenHandler : IRequestHandler<RefreshTokenCommand, Result<LoginResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly IJwtService _jwt;
    private readonly IRefreshTokenHasher _hasher;

    public RefreshTokenHandler(
        IApplicationDbContext context,
        IJwtService jwt,
        IRefreshTokenHasher hasher)
    {
        _context = context;
        _jwt = jwt;
        _hasher = hasher;
    }

    public async Task<Result<LoginResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        // =========================
        // 💣 Validation
        // =========================
        if (string.IsNullOrWhiteSpace(request.RefreshToken) || string.IsNullOrWhiteSpace(request.DeviceId))
            return Result<LoginResponse>.Failure("Invalid request");

        // =========================
        // 🔥 Load valid tokens only (IMPORTANT FIX)
        // =========================
        var tokens = await _context.RefreshTokens
            .AsNoTracking()
            .Where(x => x.ExpiresAt > DateTime.UtcNow && !x.IsRevoked)
            .ToListAsync(cancellationToken);

        // 💣 Hash verification (still needed)
        var existingToken = tokens.FirstOrDefault(t =>
            _hasher.Verify(request.RefreshToken, t.Token));

        if (existingToken == null)
            return Result<LoginResponse>.Failure("Invalid refresh token");

        // =========================
        // 🚨 Security Checks
        // =========================
        if (existingToken.DeviceInfo != request.DeviceId)
        {
            await RevokeAllUserSessions(existingToken.UserId, cancellationToken);
            return Result<LoginResponse>.Failure("Device mismatch detected");
        }

        // =========================
        // 🔥 Get User
        // =========================
        var user = await _context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == existingToken.UserId, cancellationToken);

        if (user == null || !user.IsActive || user.IsLocked)
            return Result<LoginResponse>.Failure("User invalid");

        // =========================
        // 🔥 Roles
        // =========================
        var roles = await _context.UserRoles
            .Where(ur => ur.UserId == user.Id)
            .Join(_context.Roles.IgnoreQueryFilters(),
                ur => ur.RoleId,
                r => r.Id,
                (ur, r) => r.Name)
            .ToListAsync(cancellationToken);

        // =========================
        // 🔥 Permissions
        // =========================
        var permissions = await _context.UserRoles
            .Where(ur => ur.UserId == user.Id)
            .Join(_context.RolePermissions.IgnoreQueryFilters(),
                ur => ur.RoleId,
                rp => rp.RoleId,
                (ur, rp) => rp.PermissionId)
            .Join(_context.Permissions.IgnoreQueryFilters(),
                rp => rp,
                p => p.Id,
                (rp, p) => p.Code)
            .Distinct()
            .ToListAsync(cancellationToken);

        // =========================
        // 🔐 New Tokens
        // =========================
        var newAccessToken = _jwt.GenerateToken(
            user.Id,
            user.TenantId,
            roles,
            permissions,
            user.BranchId,
            user
        );

        var rawRefreshToken = _jwt.GenerateRefreshToken();
        var hashed = _hasher.Hash(rawRefreshToken);

        // =========================
        // 🔄 Rotate Token (IMPORTANT)
        // =========================
        var tokenToUpdate = await _context.RefreshTokens
            .FirstAsync(x => x.Id == existingToken.Id, cancellationToken);

        tokenToUpdate.IsRevoked = true;
        tokenToUpdate.ReplacedByTokenHash = hashed;

        await _context.RefreshTokens.AddAsync(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = hashed,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            TenantId = user.TenantId,
            DeviceInfo = request.DeviceId
        }, cancellationToken);

        // =========================
        // 🔄 Update Session
        // =========================
        var session = await _context.UserSessions
            .FirstOrDefaultAsync(x => x.UserId == user.Id && x.DeviceId == request.DeviceId, cancellationToken);

        if (session != null)
            session.LastActivityAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return Result<LoginResponse>.Success(new LoginResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = rawRefreshToken,
            FullName = user.FullName,
            DeviceId = request.DeviceId
        });
    }

    // =========================
    // 💣 Security Method
    // =========================
    private async Task RevokeAllUserSessions(Guid userId, CancellationToken cancellationToken)
    {
        await _context.UserSessions
            .Where(x => x.UserId == userId && !x.IsRevoked)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.IsRevoked, true), cancellationToken);

        await _context.RefreshTokens
            .Where(x => x.UserId == userId && !x.IsRevoked)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.IsRevoked, true), cancellationToken);
    }
}