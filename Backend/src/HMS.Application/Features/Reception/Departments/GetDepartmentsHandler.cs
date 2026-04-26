using HMS.Application.Abstractions.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HMS.Application.Features.Reception.Departments;

public class GetDepartmentsHandler : IRequestHandler<GetDepartmentsQuery, List<DepartmentDto>>
{
    private readonly IApplicationDbContext _context;

    public GetDepartmentsHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<DepartmentDto>> Handle(GetDepartmentsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Departments
            .AsNoTracking();

        // 🔥 filter بالفرع
        if (request.BranchId.HasValue)
        {
            query = query.Where(d => d.BranchId == request.BranchId);
        }

        return await query
            .Select(d => new DepartmentDto
            {
                Id = d.Id,
                Name = d.Name,
                BranchId = d.BranchId
            })
            .ToListAsync(cancellationToken);
    }
}