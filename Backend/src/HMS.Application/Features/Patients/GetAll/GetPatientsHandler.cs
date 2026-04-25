using HMS.Application.Abstractions.Persistence;
using HMS.Application.Abstractions.CurrentUser;
using HMS.Application.Features.Patients.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HMS.Application.Features.Patients.GetAll;

public class GetPatientsHandler : IRequestHandler<GetPatientsQuery, PaginatedResult<PatientDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public GetPatientsHandler(
        IApplicationDbContext context,
        ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<PaginatedResult<PatientDto>> Handle(
        GetPatientsQuery request,
        CancellationToken cancellationToken)
    {
        var tenantId = _currentUser.TenantId;

        // =========================
        // 🧠 Base Query
        // =========================
        var query = _context.Patients
            .AsNoTracking()
            .Where(x =>
                x.TenantId == tenantId &&   // 💣 SaaS
                !x.IsDeleted);              // 💣 Soft delete

        var search = request.Search?.Trim();

        // =========================
        // 🔍 Search (Optimized)
        // =========================
        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.ToLower();

            query = query.Where(x =>
                x.FullName.ToLower().Contains(search) ||
                x.PhoneNumber.Contains(search) ||
                x.MedicalNumber.Contains(search));
        }

        // =========================
        // 📊 Count
        // =========================
        var totalCount = await query.CountAsync(cancellationToken);

        // =========================
        // 🛡️ Pagination
        // =========================
        var page = request.PageNumber <= 0 ? 1 : request.PageNumber;
        var pageSize = request.PageSize <= 0 ? 10 : request.PageSize;

        if (pageSize > 50)
            pageSize = 50; // 💣 حماية السيرفر

        // =========================
        // 📄 Data
        // =========================
        var patients = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new PatientDto
            {
                Id = x.Id,
                FullName = x.FullName,
                MedicalNumber = x.MedicalNumber,
                PhoneNumber = x.PhoneNumber
            })
            .ToListAsync(cancellationToken);

        // =========================
        // 📦 Result
        // =========================
        return new PaginatedResult<PatientDto>
        {
            PageNumber = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            Items = patients
        };
    }
}