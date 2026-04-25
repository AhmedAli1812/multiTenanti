using HMS.Application.Abstractions.Persistence;
using HMS.Application.Abstractions.Tenant;
using HMS.Application.Abstractions.CurrentUser;
using HMS.Application.Abstractions.Caching;
using HMS.Shared.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HMS.Application.Features.Users.DeleteUser;

public class DeleteUserHandler : IRequestHandler<DeleteUserCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantProvider _tenantProvider;
    private readonly ICurrentUser _currentUser;
    private readonly IPermissionCacheService _cache;

    public DeleteUserHandler(
        IApplicationDbContext context,
        ITenantProvider tenantProvider,
        ICurrentUser currentUser,
        IPermissionCacheService cache)
    {
        _context = context;
        _tenantProvider = tenantProvider;
        _currentUser = currentUser;
        _cache = cache;
    }

    public async Task<Result> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId();

        // =========================
        // 🔍 Get User (حتى لو deleted)
        // =========================
        var user = await _context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x =>
                x.Id == request.Id &&
                x.TenantId == tenantId,
                cancellationToken);

        if (user == null)
            return Result.Failure("User not found");

        // =========================
        // 🚫 Already deleted
        // =========================
        if (user.IsDeleted)
            return Result.Success(); // idempotent

        // =========================
        // 🚫 Prevent self delete
        // =========================
        if (user.Id == _currentUser.UserId)
            return Result.Failure("You cannot delete your own account");

        // =========================
        // 🚫 Check relations (اختياري حسب البزنس)
        // =========================
        var hasVisits = await _context.Visits
            .AsNoTracking()
            .AnyAsync(v =>
                v.DoctorId == user.Id &&
                v.TenantId == tenantId,
                cancellationToken);

        if (hasVisits)
            return Result.Failure("User has related visits and cannot be deleted");

        // =========================
        // 🔥 Soft Delete
        // =========================
        user.IsDeleted = true;
        user.DeletedAt = DateTime.UtcNow;
        user.DeletedBy = _currentUser.UserId;

        // =========================
        // 🔥 Revoke Tokens
        // =========================
        var tokens = await _context.RefreshTokens
            .Where(t => t.UserId == user.Id && !t.IsRevoked)
            .ToListAsync(cancellationToken);

        foreach (var t in tokens)
            t.IsRevoked = true;

        // =========================
        // 💾 Save
        // =========================
        await _context.SaveChangesAsync(cancellationToken);

        // =========================
        // 🔥 Clear Cache
        // =========================
        await _cache.RemoveAsync(user.Id);

        return Result.Success();
    }
}