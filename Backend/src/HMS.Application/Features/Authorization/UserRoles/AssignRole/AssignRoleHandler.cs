using HMS.Application.Abstractions.Persistence;
using HMS.Application.Abstractions.Tenant;
using HMS.Domain.Entities.Identity;
using HMS.Shared.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using HMS.Application.Abstractions.Caching;

namespace HMS.Application.Features.Authorization.UserRoles.AssignRole
{
    public class AssignRoleHandler : IRequestHandler<AssignRoleCommand, Result>
    {
        private readonly IApplicationDbContext _context;
        private readonly IPermissionCacheService _cache;
        private readonly ITenantProvider _tenantProvider;

        public AssignRoleHandler(
            IApplicationDbContext context,
            IPermissionCacheService cache,
            ITenantProvider tenantProvider)
        {
            _context = context;
            _cache = cache;
            _tenantProvider = tenantProvider;
        }

        public async Task<Result> Handle(AssignRoleCommand request, CancellationToken cancellationToken)
        {
            // =========================
            // 💣 Validation
            // =========================
            if (request.UserId == Guid.Empty || request.RoleId == Guid.Empty)
                return Result.Failure("Invalid request");

            var tenantId = _tenantProvider.GetTenantId();
            var isSuperAdmin = _tenantProvider.IsSuperAdmin();

            // =========================
            // 🔥 Check User
            // =========================
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.UserId && (isSuperAdmin || x.TenantId == tenantId), cancellationToken);

            if (user == null)
                return Result.Failure("User not found");

            // =========================
            // 🔥 Check Role
            // =========================
            var roleExists = await _context.Roles
                .AsNoTracking()
                .AnyAsync(x => x.Id == request.RoleId && (isSuperAdmin || x.TenantId == tenantId), cancellationToken);

            if (!roleExists)
                return Result.Failure("Role not found");

            // =========================
            // 🔥 Prevent Duplicate
            // =========================
            var alreadyAssigned = await _context.UserRoles
                .AsNoTracking()
                .AnyAsync(x =>
                    x.UserId == request.UserId &&
                    x.RoleId == request.RoleId &&
                    (isSuperAdmin || x.TenantId == tenantId),
                    cancellationToken);

            if (alreadyAssigned)
                return Result.Failure("Role already assigned to user");

            // =========================
            // 🔥 Assign Role
            // =========================
            _context.UserRoles.Add(new UserRole
            {
                UserId = request.UserId,
                RoleId = request.RoleId,
                TenantId = user.TenantId // Use the user's tenant ID
            });

            await _context.SaveChangesAsync(cancellationToken);

            // =========================
            // 🔥 Invalidate Cache
            // =========================
            await _cache.RemoveAsync(request.UserId);

            return Result.Success();
        }
    }
}