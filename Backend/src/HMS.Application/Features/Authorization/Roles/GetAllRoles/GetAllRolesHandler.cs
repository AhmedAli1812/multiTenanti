using HMS.Application.Abstractions.Persistence;
using HMS.Application.Abstractions.Tenant;
using HMS.Application.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

public class GetAllRolesHandler
    : IRequestHandler<GetAllRolesQuery, List<RoleDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantProvider _tenantProvider;

    public GetAllRolesHandler(
        IApplicationDbContext context,
        ITenantProvider tenantProvider)
    {
        _context = context;
        _tenantProvider = tenantProvider;
    }

    public async Task<List<RoleDto>> Handle(
        GetAllRolesQuery request,
        CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId();

        var query = _context.Roles.AsNoTracking();

        if (tenantId.HasValue)
        {
            query = query.Where(r => r.TenantId == tenantId);
        }

        var roles = await query
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .OrderBy(r => r.Name)
            .Select(r => new RoleDto
            {
                Id = r.Id,
                Name = r.Name,
                Permissions = r.RolePermissions.Select(rp => rp.Permission.Code).ToList()
            })
            .ToListAsync(cancellationToken);

        return roles;
    }
}