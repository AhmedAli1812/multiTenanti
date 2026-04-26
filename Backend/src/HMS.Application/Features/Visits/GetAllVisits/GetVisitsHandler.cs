using HMS.Application.Abstractions.Persistence;
using HMS.Application.Abstractions.CurrentUser;
using HMS.Application.Dtos;
using HMS.Application.Features.Patients.Common;
using HMS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HMS.Application.Features.Visits.GetVisits;

public class GetVisitsHandler : IRequestHandler<GetVisitsQuery, PaginatedResult<VisitListDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public GetVisitsHandler(
        IApplicationDbContext context,
        ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<PaginatedResult<VisitListDto>> Handle(
        GetVisitsQuery request,
        CancellationToken cancellationToken)
    {
        var page = request.PageNumber <= 0 ? 1 : request.PageNumber;
        var pageSize = request.PageSize <= 0 ? 10 : request.PageSize;
        if (pageSize > 50) pageSize = 50;

        var search = request.Search?.Trim();

        // =========================
        // 🧠 Base Query
        // =========================
        var query = _context.Visits
            .AsNoTracking();

        // 🔥 Super Admin يشوف كل الداتا
        if (_currentUser.IsGlobal)
        {
            query = query.IgnoreQueryFilters(); // مهم جدًا لو عندك Global Filter
        }

        // =========================
        // 🔍 Search
        // =========================
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(v =>
                v.Patient.FullName.Contains(search) ||
                v.Patient.MedicalNumber.Contains(search));
        }

        // =========================
        // 🎯 Status Filter
        // =========================
        if (!string.IsNullOrWhiteSpace(request.Status) &&
            Enum.TryParse<VisitStatus>(request.Status, true, out var status))
        {
            query = query.Where(v => v.Status == status);
        }

        // =========================
        // 📊 Count
        // =========================
        var totalCount = await query.CountAsync(cancellationToken);

        // =========================
        // 📄 Data
        // =========================
        var visits = await query
            .OrderByDescending(v => v.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(v => new VisitListDto
            {
                Id = v.Id,
                PatientName = v.Patient.FullName,
                MedicalNumber = v.Patient.MedicalNumber,

                DoctorName = v.Doctor != null
                    ? v.Doctor.FullName
                    : "—",

                // 🔥 Room من RoomAssignments
                Room = _context.RoomAssignments
                    .Where(a => a.VisitId == v.Id && a.IsActive)
                    .Select(a => a.Room.RoomNumber)
                    .FirstOrDefault() ?? "—",

                BranchName = v.Branch.Name,

                Status = v.Status.ToString(),

                StartedAt = v.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return new PaginatedResult<VisitListDto>
        {
            PageNumber = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            Items = visits,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }
}