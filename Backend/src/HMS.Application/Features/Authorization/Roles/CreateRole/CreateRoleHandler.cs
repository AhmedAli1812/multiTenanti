using HMS.Application.Abstractions.Persistence;
using HMS.Application.Abstractions.Tenant;
using HMS.Domain.Entities.Identity;
using HMS.Shared.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HMS.Application.Features.Authorization.Roles.CreateRole
{
    public class CreateRoleHandler : IRequestHandler<CreateRoleCommand, Result<Guid>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ITenantProvider _tenantProvider;

        public CreateRoleHandler(IApplicationDbContext context, ITenantProvider tenantProvider)
        {
            _context = context;
            _tenantProvider = tenantProvider;
        }

        public async Task<Result<Guid>> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
        {
            // =========================
            // 💣 Validation
            // =========================
            if (string.IsNullOrWhiteSpace(request.Name))
                return Result<Guid>.Failure("Role name is required");

            var roleName = request.Name.Trim();

            var tenantId = _tenantProvider.GetTenantId();

            // =========================
            // 🔥 Check duplicate
            // =========================
            var exists = await _context.Roles
                .AnyAsync(x => x.Name == roleName && x.TenantId == tenantId, cancellationToken);

            if (exists)
                return Result<Guid>.Failure("Role already exists");

            // =========================
            // 🔥 Validate permissions
            // =========================
            var permissionIds = request.PermissionIds?.Distinct().ToList() ?? new List<Guid>();

            if (permissionIds.Count > 0)
            {
                var validPermissions = await _context.Permissions
                    .Where(p => permissionIds.Contains(p.Id))
                    .Select(p => p.Id)
                    .ToListAsync(cancellationToken);

                if (validPermissions.Count != permissionIds.Count)
                    return Result<Guid>.Failure("One or more permissions are invalid");
            }

            // =========================
            // 🔥 Create Role
            // =========================
            var role = new Role
            {
                Id = Guid.NewGuid(),
                Name = roleName,
                TenantId = tenantId
            };

            await _context.Roles.AddAsync(role, cancellationToken);

            // =========================
            // 🔥 Add RolePermissions (Batch)
            // =========================
            if (permissionIds.Count > 0)
            {
                var rolePermissions = permissionIds.Select(permissionId => new RolePermission
                {
                    RoleId = role.Id,
                    PermissionId = permissionId
                });

                await _context.RolePermissions.AddRangeAsync(rolePermissions, cancellationToken);
            }

            // =========================
            // 💾 Save
            // =========================
            await _context.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(role.Id);
        }
    }
}