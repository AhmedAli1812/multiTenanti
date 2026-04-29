using HMS.Application.Abstractions.Persistence;
using HMS.Application.Abstractions.Tenant;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HMS.Application.Features.Branches.GetBranches;

public class GetBranchesHandler : IRequestHandler<GetBranchesQuery, List<BranchDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantProvider _tenantProvider;

    public GetBranchesHandler(
        IApplicationDbContext context,
        ITenantProvider tenantProvider)
    {
        _context = context;
        _tenantProvider = tenantProvider;
    }

    public async Task<List<BranchDto>> Handle(GetBranchesQuery request, CancellationToken cancellationToken)
    {
        var tenantId = (request.TenantId.HasValue && _tenantProvider.IsSuperAdmin())
            ? request.TenantId.Value
            : _tenantProvider.GetTenantId();

        var query = _context.Branches.AsNoTracking();

        if (tenantId.HasValue)
        {
            query = query.Where(b => b.TenantId == tenantId);
        }

        var branches = await query
            .OrderBy(b => b.Name)
            .Select(b => new BranchDto
            {
                Id = b.Id,
                Name = b.Name
            })
            .ToListAsync(cancellationToken);

        return branches;
    }
}