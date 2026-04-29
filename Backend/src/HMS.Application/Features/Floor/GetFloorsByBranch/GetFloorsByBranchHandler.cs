using HMS.Application.Abstractions.Persistence;
using HMS.Application.Abstractions.Tenant;
using HMS.Application.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HMS.Application.Features.Floors.GetFloorsByBranch;

public class GetFloorsByBranchHandler : IRequestHandler<GetFloorsByBranchQuery, List<FloorDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantProvider _tenant;

    public GetFloorsByBranchHandler(
        IApplicationDbContext context,
        ITenantProvider tenant)
    {
        _context = context;
        _tenant = tenant;
    }

    public async Task<List<FloorDto>> Handle(GetFloorsByBranchQuery request, CancellationToken cancellationToken)
    {
        // =========================
        // 💣 Validation
        // =========================
        if (request.BranchId == Guid.Empty)
            throw new ArgumentException("Invalid branch");

        var tenantId = _tenant.GetTenantId();
        var isSuperAdmin = _tenant.IsSuperAdmin();

        // =========================
        // 🔥 Ensure branch belongs to tenant (unless super admin)
        // =========================
        var branchQuery = _context.Branches.AsNoTracking();
        
        if (!isSuperAdmin)
        {
            branchQuery = branchQuery.Where(b => b.TenantId == tenantId);
        }

        var branch = await branchQuery
            .FirstOrDefaultAsync(b => b.Id == request.BranchId, cancellationToken);

        if (branch == null)
            throw new InvalidOperationException("Branch not found");

        var effectiveTenantId = branch.TenantId;

        // =========================
        // 🔥 Get Floors
        // =========================
        var floors = await _context.Floors
            .AsNoTracking() // ⚡ performance
            .Where(f =>
                f.BranchId == request.BranchId &&
                f.TenantId == effectiveTenantId // 💣 مهم جدًا
            )
            .OrderBy(f => f.Number)
            .Select(f => new FloorDto
            {
                Id = f.Id,
                Name = f.Name,
                Number = f.Number
            })
            .ToListAsync(cancellationToken);

        return floors;
    }
}