using HMS.Application.Abstractions.Persistence;
using HMS.Application.Abstractions.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

public class SearchPatientsHandler
    : IRequestHandler<SearchPatientsQuery, List<PatientSearchDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public SearchPatientsHandler(
        IApplicationDbContext context,
        ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<PatientSearchDto>> Handle(
        SearchPatientsQuery request,
        CancellationToken cancellationToken)
    {
        var tenantId = _currentUser.TenantId;

        var query = _context.Patients
            .AsNoTracking()
            .Where(p =>
                p.TenantId == tenantId &&   // 💣 SaaS
                !p.IsDeleted);              // 💣 Soft delete

        var term = request.Term?.Trim();

        if (!string.IsNullOrWhiteSpace(term))
        {
            term = term.ToLower();

            query = query.Where(p =>
                p.FullName.ToLower().Contains(term) ||
                p.MedicalNumber.Contains(term) ||
                p.PhoneNumber.Contains(term));
        }

        return await query
            .OrderBy(p => p.FullName) // 👌 UX أفضل
            .Select(p => new PatientSearchDto
            {
                Id = p.Id,
                Name = p.FullName,
                MedicalNumber = p.MedicalNumber
            })
            .Take(20) // ⚡ limit مهم للأداء
            .ToListAsync(cancellationToken);
    }
}