using HMS.Application.Abstractions.Persistence;
using HMS.Application.Features.ReceptionDashboard.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;
using HMS.Domain.Enums;

namespace HMS.Application.Features.ReceptionDashboard.Queries;

public class GetReceptionDashboardHandler
    : IRequestHandler<GetReceptionDashboardQuery, ReceptionDashboardDto>
{
    private readonly IApplicationDbContext _context;

    public GetReceptionDashboardHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ReceptionDashboardDto> Handle(
        GetReceptionDashboardQuery request,
        CancellationToken ct)
    {
        // =============================
        // 🔥 BASE QUERY
        // =============================
        var query = _context.Visits
            .AsNoTracking()
            .Where(v => v.TenantId == request.TenantId);

        // =============================
        // 🔍 FILTERS
        // =============================
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();

            query = query.Where(v =>
                v.Patient.FullName.Contains(search) ||
                v.Patient.MedicalNumber.Contains(search));
        }

        if (request.FromDate.HasValue)
            query = query.Where(v => v.VisitDate >= request.FromDate);

        if (request.ToDate.HasValue)
            query = query.Where(v => v.VisitDate <= request.ToDate);

        // =============================
        // 📊 KPI
        // =============================
        var totalPatients = await _context.Patients
            .CountAsync(p => p.TenantId == request.TenantId, ct);

        var activeVisitsCount = await query
            .CountAsync(v => v.Status != VisitStatus.Completed, ct);

        var emergencyCount = await query
            .CountAsync(v => v.VisitType == VisitType.Emergency, ct);

        // 🔥 occupied rooms من RoomAssignments
        var occupiedRoomsCount = await _context.RoomAssignments
            .AsNoTracking()
            .Where(a => a.IsActive)
            .Join(_context.Visits,
                a => a.VisitId,
                v => v.Id,
                (a, v) => new { a, v })
            .CountAsync(x =>
                x.v.TenantId == request.TenantId &&
                x.v.Status != VisitStatus.Completed,
                ct);

        // =============================
        // 📄 PAGINATION
        // =============================
        var totalCount = await query.CountAsync(ct);

        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 10 : request.PageSize;

        var pagedVisits = await query
            .OrderByDescending(v => v.VisitDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(v => new
            {
                v.Id,
                v.Status,
                v.VisitDate,
                v.CompletedAt,

                PatientName = v.Patient.FullName,
                MedicalNumber = v.Patient.MedicalNumber,
                DoctorName = v.Doctor != null ? v.Doctor.FullName : "-",

                // 🔥 Room من RoomAssignments
                RoomName = _context.RoomAssignments
                    .Where(a => a.VisitId == v.Id && a.IsActive)
                    .Select(a => a.Room.RoomNumber)
                    .FirstOrDefault()
            })
            .ToListAsync(ct);

        // =============================
        // 👤 Previous Patients
        // =============================
        var previousPatients = await query
            .Where(v => v.Status == VisitStatus.Completed)
            .OrderByDescending(v => v.CompletedAt)
            .Take(10)
            .Select(v => new PreviousPatientDto
            {
                PatientName = v.Patient.FullName,
                MedicalNumber = v.Patient.MedicalNumber,
                DoctorName = v.Doctor != null ? v.Doctor.FullName : "-",
                AdmissionDate = v.VisitDate,
                DischargeDate = v.CompletedAt,
                Status = "Discharged"
            })
            .ToListAsync(ct);

        // =============================
        // 🏥 Rooms
        // =============================
        var rooms = pagedVisits
            .Where(v => v.RoomName != null && v.Status != VisitStatus.Completed)
            .Select(v => new RoomStatusDto
            {
                RoomName = v.RoomName ?? "-",
                PatientName = v.PatientName ?? "",
                DoctorName = v.DoctorName ?? "-",
                Status = v.Status.ToString()
            })
            .ToList();

        // =============================
        // 🔥 RESPONSE
        // =============================
        return new ReceptionDashboardDto
        {
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,

            Kpis = new DashboardKpiDto
            {
                TotalPatients = totalPatients,
                ActiveVisits = activeVisitsCount,
                OccupiedRooms = occupiedRoomsCount,
                EmergencyCases = emergencyCount
            },

            PreviousPatients = previousPatients,
            Rooms = rooms
        };
    }
}