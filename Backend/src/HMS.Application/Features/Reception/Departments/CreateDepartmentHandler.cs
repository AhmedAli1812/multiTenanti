using HMS.Application.Abstractions.Persistence;
using HMS.Application.Abstractions.Tenant;
using HMS.Domain.Entities.Departments;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HMS.Application.Features.Reception.Departments;

public class CreateDepartmentHandler : IRequestHandler<CreateDepartmentCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantProvider _tenant;

    public CreateDepartmentHandler(IApplicationDbContext context, ITenantProvider tenant)
    {
        _context = context;
        _tenant = tenant;
    }

    public async Task<Guid> Handle(CreateDepartmentCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Department name is required");

        if (request.BranchId == Guid.Empty)
            throw new ArgumentException("Branch is required");

        var tenantId = (request.TenantId.HasValue && _tenant.IsSuperAdmin())
            ? request.TenantId.Value
            : _tenant.GetTenantId();

        if (tenantId == null)
            throw new ArgumentException("Tenant ID is required");

        var name = request.Name.Trim();

        // Check Branch exists
        var branchExists = await _context.Branches
            .AsNoTracking()
            .AnyAsync(b => b.Id == request.BranchId && b.TenantId == tenantId, cancellationToken);

        if (!branchExists)
            throw new InvalidOperationException("Branch not found");

        // Prevent duplicate department name in same branch
        var exists = await _context.Departments
            .AsNoTracking()
            .AnyAsync(d =>
                d.BranchId == request.BranchId &&
                d.Name == name &&
                d.TenantId == tenantId,
                cancellationToken);

        if (exists)
            throw new InvalidOperationException("Department already exists in this branch");

        var department = new Department
        {
            Id = Guid.NewGuid(),
            Name = name,
            BranchId = request.BranchId,
            TenantId = tenantId
        };

        await _context.Departments.AddAsync(department, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return department.Id;
    }
}
