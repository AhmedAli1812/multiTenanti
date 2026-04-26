using HMS.Application.Abstractions.Persistence;
using HMS.Application.Abstractions.Tenant;
using HMS.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HMS.Application.Features.Floors.CreateFloor;

public class CreateFloorHandler : IRequestHandler<CreateFloorCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantProvider _tenant;

    public CreateFloorHandler(IApplicationDbContext context, ITenantProvider tenant)
    {
        _context = context;
        _tenant = tenant;
    }

    public async Task<Guid> Handle(CreateFloorCommand request, CancellationToken cancellationToken)
    {
        // =========================
        // 💣 Validation
        // =========================
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Floor name is required");

        if (request.Number < 0)
            throw new ArgumentException("Invalid floor number");

        if (request.BranchId == Guid.Empty)
            throw new ArgumentException("Branch is required");

        var tenantId = _tenant.GetTenantId();
        var name = request.Name.Trim();

        // =========================
        // 🔥 Check Branch exists
        // =========================
        var branchExists = await _context.Branches
            .AsNoTracking()
            .AnyAsync(b => b.Id == request.BranchId && b.TenantId == tenantId, cancellationToken);

        if (!branchExists)
            throw new InvalidOperationException("Branch not found");

        // =========================
        // 🔥 Prevent duplicate floor number in same branch
        // =========================
        var exists = await _context.Floors
            .AsNoTracking()
            .AnyAsync(f =>
                f.BranchId == request.BranchId &&
                f.Number == request.Number &&
                f.TenantId == tenantId,
                cancellationToken);

        if (exists)
            throw new InvalidOperationException("Floor number already exists in this branch");

        // =========================
        // 🔥 Create Floor
        // =========================
        var floor = new Floor
        {
            Id = Guid.NewGuid(),
            Name = name,
            Number = request.Number,
            BranchId = request.BranchId,
            TenantId = tenantId // 💣 مهم جدًا
        };

        await _context.Floors.AddAsync(floor, cancellationToken);

        // =========================
        // 💾 Save
        // =========================
        await _context.SaveChangesAsync(cancellationToken);

        return floor.Id;
    }
}