using HMS.Application.Abstractions.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HMS.Application.Features.Reception.Doctors;

public class GetDoctorsHandler : IRequestHandler<GetDoctorsQuery, List<DoctorLookupDto>>
{
    private readonly IApplicationDbContext _context;

    public GetDoctorsHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<DoctorLookupDto>> Handle(GetDoctorsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Users
            .AsNoTracking()
            .Where(u => u.UserRoles.Any(r => r.Role.Name == "Doctor"));
        return await query
            .Select(u => new DoctorLookupDto
            {
                Id = u.Id,
                Name = u.FullName
            })
            .ToListAsync(cancellationToken);
    }
}