using HMS.Identity.Application.Abstractions;
using HMS.Identity.Domain.Entities;
using HMS.SharedKernel.Application.Abstractions;
using HMS.SharedKernel.Primitives;
using Microsoft.EntityFrameworkCore;

namespace HMS.Identity.Application.Features.Auth.Login;

public sealed class LoginCommandHandler(
    IIdentityDbContext context,
    IPasswordHasher    passwordHasher,
    IJwtService        jwtService)
    : ICommandHandler<LoginCommand, LoginResponse>
{
    public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // ── Find user by any identifier ────────────────────────────────────────
        var identifier = request.Identifier.Trim().ToLowerInvariant();

        var user = await context.Users
            .AsNoTracking()
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u =>
                !u.IsDeleted && (
                    (u.Email != null      && u.Email.ToLower()       == identifier) ||
                    (u.PhoneNumber != null && u.PhoneNumber            == request.Identifier.Trim()) ||
                    (u.Username != null   && u.Username.ToLower()     == identifier) ||
                    (u.NationalId != null && u.NationalId              == request.Identifier.Trim())
                ), cancellationToken);

        if (user is null)
            throw new DomainException("Invalid credentials.", "INVALID_CREDENTIALS");

        if (!user.IsActive)
            throw new DomainException("Account is deactivated.", "ACCOUNT_INACTIVE");

        if (user.IsLocked)
            throw new DomainException("Account is locked.", "ACCOUNT_LOCKED");

        if (!passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new DomainException("Invalid credentials.", "INVALID_CREDENTIALS");

        // ── Fetch permissions ──────────────────────────────────────────────────
        var roleIds = user.UserRoles.Select(ur => ur.RoleId).ToList();

        var permissions = await context.RolePermissions
            .AsNoTracking()
            .Where(rp => roleIds.Contains(rp.RoleId))
            .Select(rp => rp.Permission.Name)
            .Distinct()
            .ToListAsync(cancellationToken);

        var roles = user.UserRoles
            .Select(ur => ur.Role.Name)
            .ToList();

        // ── Generate tokens ────────────────────────────────────────────────────
        var accessToken  = jwtService.GenerateAccessToken(user, roles, permissions);
        var refreshToken = jwtService.GenerateRefreshToken();

        // ── Persist refresh token ──────────────────────────────────────────────
        var refreshTokenEntity = new RefreshToken
        {
            Token     = refreshToken,
            DeviceId  = request.DeviceId,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            UserId    = user.Id,
            CreatedAt = DateTime.UtcNow,
        };
        await context.RefreshTokens.AddAsync(refreshTokenEntity, cancellationToken);

        // ── Record login (track lastLoginAt via tracked entity) ────────────────
        var tracked = await context.Users.FindAsync([user.Id], cancellationToken);
        tracked?.RecordLogin();

        await context.SaveChangesAsync(cancellationToken);

        return new LoginResponse(
            AccessToken:  accessToken,
            RefreshToken: refreshToken,
            FullName:     user.FullName,
            Role:         roles.FirstOrDefault() ?? "Unknown",
            UserId:       user.Id,
            TenantId:     user.TenantId == Guid.Empty ? null : user.TenantId,
            BranchId:     user.BranchId,
            Email:        user.Email);
    }
}
