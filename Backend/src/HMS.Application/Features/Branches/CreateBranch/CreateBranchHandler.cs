using HMS.Application.Abstractions.Persistence;
using HMS.Application.Abstractions.Tenant;
using HMS.Domain.Entities.Branches;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HMS.Application.Features.Branches.CreateBranch;

public class CreateBranchHandler : IRequestHandler<CreateBranchCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantProvider _tenantProvider;

    public CreateBranchHandler(
        IApplicationDbContext context,
        ITenantProvider tenantProvider)
    {
        _context = context;
        _tenantProvider = tenantProvider;
    }

    public async Task<Guid> Handle(CreateBranchCommand request, CancellationToken cancellationToken)
    {
        // =========================
        // 💣 Validation
        // =========================
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Branch name is required");

        var name = request.Name.Trim();

        // =========================
        // 🔥 Tenant Handling
        // =========================
        var tenantId = _tenantProvider.GetTenantId();

        if (!tenantId.HasValue)
            throw new Exception("Tenant is required to create a branch");

        var tenantGuid = tenantId.Value;

        // =========================
        // 🔥 Prevent duplicate
        // =========================
        var exists = await _context.Branches
            .AnyAsync(x => x.Name == name && x.TenantId == tenantGuid, cancellationToken);

        if (exists)
            throw new InvalidOperationException("Branch already exists");

        // =========================
        // 🔥 Create Branch
        // =========================
        var branch = new Branch
        {
            Id = Guid.NewGuid(),
            Name = name,
            TenantId = tenantGuid
        };

        await _context.Branches.AddAsync(branch, cancellationToken);

        // =========================
        // 💾 Save
        // =========================
        await _context.SaveChangesAsync(cancellationToken);

        return branch.Id;
    }
}

