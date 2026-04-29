using HMS.Application.Abstractions.Persistence;
using HMS.Domain.Entities.Tenancy;
using HMS.Shared.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HMS.Application.Features.Tenants.CreateTenant;

public class CreateTenantHandler : IRequestHandler<CreateTenantCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;

    public CreateTenantHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(CreateTenantCommand request, CancellationToken cancellationToken)
    {
        // 1. Check if code already exists
        var exists = await _context.Tenants
            .AnyAsync(t => t.Code == request.Code, cancellationToken);

        if (exists)
            return Result<Guid>.Failure("Tenant code already exists");

        // 2. Create tenant
        var tenant = new Tenant
        {
            Name = request.Name,
            Code = request.Code,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.Tenants.Add(tenant);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(tenant.Id);
    }
}
