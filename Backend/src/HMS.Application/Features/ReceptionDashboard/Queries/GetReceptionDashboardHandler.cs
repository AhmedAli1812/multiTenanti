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
            var search = request.Search.Trim().ToLower();

            query = query.Where(v =>
                v.Patient.FullName.ToLower().Contains(search) ||
                v.Patient.MedicalNumber.ToLower().Contains(search));
        }

        if (request.DepartmentId.HasValue)
        {
            query = query.Where(v => v.Doctor != null && v.Doctor.DepartmentId == request.DepartmentId);
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
            .CountAsync(v => 
                v.VisitType == VisitType.Emergency || 
                v.Priority == PriorityLevel.Emergency, ct);

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
        // 📄 PAGINATION & DATA FETCH
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

                PatientName = v.Patient != null ? v.Patient.FullName : null,
                MedicalNumber = v.Patient != null ? v.Patient.MedicalNumber : null,
                PatientDob = v.Patient != null ? (DateTime?)v.Patient.DateOfBirth : null,
                PatientGender = v.Patient != null ? (Gender?)v.Patient.Gender : null,
                DoctorName = v.Doctor != null ? v.Doctor.FullName : "-",

                // 🔥 Room من RoomAssignments
                RoomName = _context.RoomAssignments
                    .Where(a => a.VisitId == v.Id && a.IsActive)
                    .Select(a => a.Room != null ? a.Room.RoomNumber : null)
                    .FirstOrDefault(),

                DepartmentName = v.Doctor != null && v.Doctor.Department != null ? v.Doctor.Department.Name : "-",
                ChiefComplaint = v.ChiefComplaint ?? "-",
                Notes = v.Notes
            })
            .ToListAsync(ct);

        // =============================
        // 🏥 Rooms (Mapping in-memory)
        // =============================
        var today = DateTime.UtcNow;
        var rooms = pagedVisits
            .Where(v => v.Status != VisitStatus.Completed)
            .Select(v => {
                var age = v.PatientDob.HasValue ? today.Year - v.PatientDob.Value.Year : 0;
                if (v.PatientDob.HasValue && v.PatientDob.Value > today.AddYears(-age)) age--;

                return new RoomStatusDto
                {
                    VisitId = v.Id,
                    RoomName = v.RoomName ?? "-",
                    PatientName = v.PatientName ?? "",
                    PatientMedicalNumber = v.MedicalNumber ?? "",
                    DoctorName = v.DoctorName ?? "-",
                    DepartmentName = v.DepartmentName ?? "-",
                    ChiefComplaint = v.ChiefComplaint ?? "-",
                    Notes = v.Notes ?? "-",
                    Diagnosis = v.ChiefComplaint ?? "-",
                    Age = age,
                    Gender = v.PatientGender?.ToString() ?? "-",
                    Status = v.Status.ToString()
                };
            })
            .ToList();

        // =============================
        // 👤 Previous Patients
        // =============================
        var previousPatientsQuery = await query
            .Where(v => v.Status == VisitStatus.Completed)
            .OrderByDescending(v => v.CompletedAt)
            .Take(10)
            .Select(v => new 
            {
                v.Patient.FullName,
                v.Patient.MedicalNumber,
                DoctorName = v.Doctor != null ? v.Doctor.FullName : "-",
                AdmissionDate = v.VisitDate,
                DischargeDate = v.CompletedAt,
                DepartmentName = v.Doctor != null && v.Doctor.Department != null ? v.Doctor.Department.Name : "-",
                ChiefComplaint = v.ChiefComplaint ?? "-",
                Notes = v.Notes,
                Diagnosis = v.ChiefComplaint ?? "-",
                v.Patient.DateOfBirth,
                v.Patient.Gender
            })
            .ToListAsync(ct);

        var previousPatients = previousPatientsQuery.Select(p => {
            var age = today.Year - p.DateOfBirth.Year;
            if (p.DateOfBirth > today.AddYears(-age)) age--;

            return new PreviousPatientDto
            {
                PatientName = p.FullName,
                MedicalNumber = p.MedicalNumber,
                DoctorName = p.DoctorName,
                AdmissionDate = p.AdmissionDate,
                DischargeDate = p.DischargeDate,
                DepartmentName = p.DepartmentName,
                ChiefComplaint = p.ChiefComplaint,
                Notes = p.Notes,
                Diagnosis = p.Diagnosis,
                Age = age,
                Gender = p.Gender.ToString(),
                Status = "Discharged"
            };
        }).ToList();

        // =============================
        // 🏥 Departments (Dynamic)
        // =============================
        var departments = await _context.Departments
            .Where(d => d.TenantId == request.TenantId)
            .Select(d => d.Name)
            .Distinct()
            .ToListAsync(ct);

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
            Rooms = rooms,
            Departments = departments
        };
    }
}