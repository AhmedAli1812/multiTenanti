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

        var roles = await _context.Roles
            .AsNoTracking() // 🔥 performance
            .Where(r => r.TenantId == tenantId) // 💣 multi-tenant isolation
            .OrderBy(r => r.Name) // 👌 UX
            .Select(r => new RoleDto
            {
                Id = r.Id,
                Name = r.Name
            })
            .ToListAsync(cancellationToken);

        return roles;
    }
}