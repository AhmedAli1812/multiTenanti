using HMS.Application.Abstractions.Auth;
using HMS.Application.Abstractions.Persistence;
using HMS.Application.Abstractions.Security;
using HMS.Application.Abstractions.Tenant;
using HMS.Domain.Entities.Identity;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HMS.Application.Features.Auth.Login;

public class LoginHandler : IRequestHandler<LoginCommand, LoginResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _hasher;
    private readonly IJwtService _jwt;
    private readonly IRefreshTokenHasher _refreshHasher;
    private readonly ITenantProvider _tenantProvider;
    private readonly IRequestInfoProvider _requestInfo;

    public LoginHandler(
        IApplicationDbContext context,
        IPasswordHasher hasher,
        IJwtService jwt,
        IRefreshTokenHasher refreshHasher,
        ITenantProvider tenantProvider,
        IRequestInfoProvider requestInfo)
    {
        _context = context;
        _hasher = hasher;
        _jwt = jwt;
        _refreshHasher = refreshHasher;
        _tenantProvider = tenantProvider;
        _requestInfo = requestInfo;
    }

    public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // =========================
        // 🔥 VALIDATION
        // =========================
        if (string.IsNullOrWhiteSpace(request.Identifier) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            throw new UnauthorizedAccessException("Invalid credentials");
        }

        var identifier = request.Identifier.Trim().ToLower();

        // =========================
        // 🔥 GET USER
        // =========================
        var user = await _context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u =>
                (u.Email != null && u.Email.ToLower() == identifier) ||
                u.PhoneNumber == identifier ||
                (u.Username != null && u.Username.ToLower() == identifier) ||
                u.NationalId == identifier,
                cancellationToken);

        if (user == null || !_hasher.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid credentials");

        if (!user.IsActive || user.IsLocked || user.IsDeleted)
            throw new UnauthorizedAccessException("User is inactive or locked");

        // =========================
        // 🔥 ROLES
        // =========================
        var roles = await _context.UserRoles
            .IgnoreQueryFilters()
            .Where(ur => ur.UserId == user.Id)
            .Join(_context.Roles.IgnoreQueryFilters(),
                ur => ur.RoleId,
                r => r.Id,
                (ur, r) => r.Name)
            .ToListAsync(cancellationToken);

        var isSuperAdmin = roles.Contains("Super Admin");

        // =========================
        // 💣 TENANT
        // =========================
        Guid? tenantId = null;

        if (!isSuperAdmin)
        {
            if (user.TenantId == Guid.Empty)
                throw new Exception("Tenant is required");

            tenantId = user.TenantId;

            _tenantProvider.SetTenantId(user.TenantId.Value);
        }

        // =========================
        // 🌍 REQUEST INFO
        // =========================
        var ip = _requestInfo.GetIpAddress();
        var userAgent = _requestInfo.GetUserAgent();

        // =========================
        // 🔥 PERMISSIONS
        // =========================
        var permissions = await _context.UserRoles
            .IgnoreQueryFilters()
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
        // 🚨 VALIDATION (IMPORTANT)
        // =========================
        if (!isSuperAdmin && user.BranchId == null)
            throw new Exception("User must be assigned to a branch");

        // =========================
        // 🔐 ACCESS TOKEN (FIXED)
        // =========================
        var accessToken = _jwt.GenerateToken(
            user.Id,
            tenantId,
            roles,
            permissions,
            user.BranchId, // 🔥 لازم نضيف ده
            user
        );

        // =========================
        // 🔄 SESSION CLEANUP
        // =========================
        var existingSessions = _context.UserSessions
            .Where(x => x.UserId == user.Id);

        _context.UserSessions.RemoveRange(existingSessions);

        var existingTokens = await _context.RefreshTokens
            .Where(x => x.UserId == user.Id && !x.IsRevoked)
            .ToListAsync(cancellationToken);

        foreach (var token in existingTokens)
        {
            token.IsRevoked = true;
        }

        // =========================
        // 🔐 REFRESH TOKEN
        // =========================
        var rawRefreshToken = _jwt.GenerateRefreshToken();
        var hashedToken = _refreshHasher.Hash(rawRefreshToken);

        var deviceId = Guid.NewGuid().ToString();

        var session = new UserSession
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            DeviceId = deviceId,
            DeviceName = "Unknown",
            IpAddress = ip,
            UserAgent = userAgent,
            LastActivityAt = DateTime.UtcNow,
            TenantId = tenantId
        };

        await _context.UserSessions.AddAsync(session, cancellationToken);

        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = hashedToken,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow,
            TenantId = tenantId,
            DeviceInfo = deviceId
        };

        await _context.RefreshTokens.AddAsync(refreshToken, cancellationToken);

        // =========================
        // 🔄 UPDATE USER
        // =========================
        user.LastLoginAt = DateTime.UtcNow;

        // =========================
        // 💾 SAVE
        // =========================
        await _context.SaveChangesAsync(cancellationToken);

        return new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = rawRefreshToken,
            FullName = user.FullName,
            DeviceId = deviceId
        };
    }
}