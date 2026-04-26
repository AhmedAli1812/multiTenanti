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

        if (request.RoleId == Guid.Empty)
            throw new ArgumentException("Invalid role");

        if (request.PermissionIds == null || !request.PermissionIds.Any())
            return true;

        var permissionIds = request.PermissionIds.Distinct().ToList();

        // =========================
        // 🔥 Base Query (Global aware)
        // =========================
        var rolesQuery = _context.Roles.AsNoTracking();

        if (_currentUser.IsGlobal)
        {
            rolesQuery = rolesQuery.IgnoreQueryFilters(); // 💣
        }

        var role = await rolesQuery
            .FirstOrDefaultAsync(r => r.Id == request.RoleId, cancellationToken);

        if (role == null)
            throw new InvalidOperationException("Role not found");

        // =========================
        // 🔥 Non-global لازم نفس التينانت
        // =========================
        if (!_currentUser.IsGlobal && role.TenantId != tenantId)
            throw new UnauthorizedAccessException("Access denied");

        // =========================
        // 🔥 RolePermissions Query
        // =========================
        var rolePermissionsQuery = _context.RolePermissions.AsNoTracking();

        if (_currentUser.IsGlobal)
        {
            rolePermissionsQuery = rolePermissionsQuery.IgnoreQueryFilters();
        }

        var existingPermissionIds = await rolePermissionsQuery
            .Where(rp => rp.RoleId == request.RoleId)
            .Select(rp => rp.PermissionId)
            .ToListAsync(cancellationToken);

        // =========================
        // 🔥 Filter new permissions
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
            TenantId = role.TenantId // 💣 مهم جدًا
        });

        await _context.RolePermissions.AddRangeAsync(entities, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        await _cache.RemoveRoleAsync(request.RoleId);

        return true;
    }
}