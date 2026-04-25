using HMS.Application.Abstractions.Persistence;
using HMS.Application.Abstractions.CurrentUser;
using HMS.Domain.Entities.Identity;
using MediatR;
using Microsoft.EntityFrameworkCore;
using HMS.Application.Abstractions.Caching;

namespace HMS.Application.Features.Permissions;

public class AssignPermissionsToRoleHandler
    : IRequestHandler<AssignPermissionsToRoleCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;
    private readonly IPermissionCacheService _cache;

    public AssignPermissionsToRoleHandler(
        IApplicationDbContext context,
        ICurrentUser currentUser,
        IPermissionCacheService cache)
    {
        _context = context;
        _currentUser = currentUser;
        _cache = cache;
    }

    public async Task<bool> Handle(
        AssignPermissionsToRoleCommand request,
        CancellationToken cancellationToken)
    {
        var tenantId = _currentUser.TenantId;

        // =========================
        // 💣 Validation
        // =========================
        if (request.RoleId == Guid.Empty)
            throw new ArgumentException("Invalid role");

        if (request.PermissionIds == null || !request.PermissionIds.Any())
            return true;

        // remove duplicates from request
        var permissionIds = request.PermissionIds.Distinct().ToList();

        // =========================
        // 🔥 Ensure role belongs to tenant
        // =========================
        var roleExists = await _context.Roles
            .AsNoTracking()
            .AnyAsync(r => r.Id == request.RoleId && r.TenantId == tenantId, cancellationToken);

        if (!roleExists)
            throw new InvalidOperationException("Role not found");

        // =========================
        // 🔥 Get existing permissions مرة واحدة
        // =========================
        var existingPermissionIds = await _context.RolePermissions
            .AsNoTracking()
            .Where(rp =>
                rp.RoleId == request.RoleId &&
                rp.TenantId == tenantId)
            .Select(rp => rp.PermissionId)
            .ToListAsync(cancellationToken);

        // =========================
        // 🔥 Filter new permissions فقط
        // =========================
        var newPermissions = permissionIds
            .Except(existingPermissionIds)
            .ToList();

        if (!newPermissions.Any())
            return true;

        // =========================
        // 🔥 Add batch
        // =========================
        var entities = newPermissions.Select(pid => new RolePermission
        {
            RoleId = request.RoleId,
            PermissionId = pid,
            TenantId = tenantId
        });

        await _context.RolePermissions.AddRangeAsync(entities, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        // =========================
        // 🔥 Clear cache
        // =========================
        await _cache.RemoveRoleAsync(request.RoleId);

        return true;
    }
}