using HMS.Application.Abstractions.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HMS.Application.Features.Tenants.GetTenants;

public class GetTenantsHandler : IRequestHandler<GetTenantsQuery, List<TenantDto>>
{
    private readonly IApplicationDbContext _context;

    public GetTenantsHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<TenantDto>> Handle(GetTenantsQuery request, CancellationToken cancellationToken)
    {
        var tenants = await _context.Tenants
            .IgnoreQueryFilters()
            .AsNoTracking()
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new TenantDto
            {
                Id = t.Id,
                Name = t.Name,
                Code = t.Code,
                IsActive = t.IsActive,
                CreatedAt = t.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return tenants;
    }
}
