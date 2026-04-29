using HMS.Application.Abstractions.Persistence;
using HMS.Application.Abstractions.CurrentUser;
using HMS.Domain.Entities.Identity;
using MediatR;
using Microsoft.EntityFrameworkCore;
using HMS.Application.Abstractions.Caching;
using HMS.Application.Abstractions.Tenant;

namespace HMS.Application.Features.Permissions;

public class AssignPermissionsToRoleHandler
    : IRequestHandler<AssignPermissionsToRoleCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;
    private readonly IPermissionCacheService _cache;
    private readonly ITenantProvider _tenantProvider;

    public AssignPermissionsToRoleHandler(
        IApplicationDbContext context,
        ICurrentUser currentUser,
        IPermissionCacheService cache,
        ITenantProvider tenantProvider)
    {
        _context = context;
        _currentUser = currentUser;
        _cache = cache;
        _tenantProvider = tenantProvider;
    }

    public async Task<bool> Handle(
     AssignPermissionsToRoleCommand request,
     CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId();
        var isSuperAdmin = _tenantProvider.IsSuperAdmin();

        if (request.RoleId == Guid.Empty)
            throw new ArgumentException("Invalid role");

        var permissionIds = (request.PermissionIds ?? new List<Guid>()).Distinct().ToList();

        // =========================
        // 🔥 Base Query (Global aware)
        // =========================
        var rolesQuery = _context.Roles.AsNoTracking();

        if (isSuperAdmin)
        {
            rolesQuery = rolesQuery.IgnoreQueryFilters();
        }

        var role = await rolesQuery
            .FirstOrDefaultAsync(r => r.Id == request.RoleId, cancellationToken);

        if (role == null)
            throw new InvalidOperationException("Role not found");

        // =========================
        // 🔥 Non-global must be same tenant
        // =========================
        if (!isSuperAdmin && role.TenantId != tenantId)
            throw new UnauthorizedAccessException("Access denied");

        // =========================
        // 🔥 Get existing
        // =========================
        var rolePermissionsQuery = _context.RolePermissions.AsTracking(); // Using tracking to delete easily

        if (isSuperAdmin)
        {
            rolePermissionsQuery = rolePermissionsQuery.IgnoreQueryFilters();
        }

        var existingEntities = await rolePermissionsQuery
            .Where(rp => rp.RoleId == request.RoleId)
            .ToListAsync(cancellationToken);

        var existingPermissionIds = existingEntities.Select(rp => rp.PermissionId).ToList();

        // =========================
        // 🔥 Sync Logic
        // =========================
        
        // 1. Remove unselected
        var toRemove = existingEntities
            .Where(rp => !permissionIds.Contains(rp.PermissionId))
            .ToList();
        
        if (toRemove.Any())
        {
            _context.RolePermissions.RemoveRange(toRemove);
        }

        // 2. Add new
        var toAddIds = permissionIds
            .Except(existingPermissionIds)
            .ToList();

        if (toAddIds.Any())
        {
            var newEntities = toAddIds.Select(pid => new RolePermission
            {
                RoleId = request.RoleId,
                PermissionId = pid,
                TenantId = role.TenantId
            });
            await _context.RolePermissions.AddRangeAsync(newEntities, cancellationToken);
        }

        await _context.SaveChangesAsync(cancellationToken);

        await _cache.RemoveRoleAsync(request.RoleId);

        return true;
    }
}