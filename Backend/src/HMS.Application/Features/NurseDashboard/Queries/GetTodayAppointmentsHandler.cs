using HMS.Application.Abstractions.Persistence;
using HMS.Application.Features.NurseDashboard.Dtos;
using HMS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HMS.Application.Features.NurseDashboard.Queries;

public class GetTodayAppointmentsHandler : IRequestHandler<GetTodayAppointmentsQuery, List<TodayAppointmentDto>>
{
    private readonly IApplicationDbContext _context;

    public GetTodayAppointmentsHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<TodayAppointmentDto>> Handle(GetTodayAppointmentsQuery request, CancellationToken ct)
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);
        var now = DateTime.UtcNow;

        var visits = await _context.Visits
            .AsNoTracking()
            .Where(v => v.TenantId == request.TenantId
                     && v.VisitDate >= today
                     && v.VisitDate < tomorrow)
            .OrderBy(v => v.VisitDate)
            .Select(v => new
            {
                v.Id,
                v.VisitDate,
                v.Status,
                v.VisitType,
                v.QueueNumber,
                PatientName = v.Patient.FullName,
                DoctorName = v.Doctor != null ? v.Doctor.FullName : "-",
                DepartmentName = v.Doctor != null && v.Doctor.Department != null
                    ? v.Doctor.Department.Name : "-",
                v.ChiefComplaint,
                v.Notes
            })
            .ToListAsync(ct);

        return visits.Select(v => new TodayAppointmentDto
        {
            VisitId = v.Id,
            PatientName = v.PatientName ?? "",
            DoctorName = v.DoctorName ?? "-",
            DepartmentName = v.DepartmentName ?? "-",
            ScheduledTime = v.VisitDate,
            QueueNumber = v.QueueNumber,
            Status = ComputeAppointmentStatus(v.Status, v.VisitDate, now),
            ChiefComplaint = v.ChiefComplaint,
            Notes = v.Notes,
            VisitTypeName = MapVisitType(v.VisitType)
        }).ToList();
    }

    private static string ComputeAppointmentStatus(VisitStatus status, DateTime visitDate, DateTime now)
    {
        if (status == VisitStatus.Completed) return "مكتمل";

        if (status == VisitStatus.WaitingDoctor ||
            status == VisitStatus.Prepared ||
            status == VisitStatus.InOp ||
            status == VisitStatus.OpCompleted ||
            status == VisitStatus.PostOp)
            return "حاضر";

        if (status == VisitStatus.CheckedIn && visitDate < now.AddMinutes(-15))
            return "متأخر";

        if (visitDate > now) return "قادم";

        return "حاضر";
    }

    private static string MapVisitType(VisitType type) => type switch
    {
        VisitType.Outpatient => "حجز",
        VisitType.Emergency  => "طارئ",
        VisitType.Inpatient  => "راجع",
        _                    => "حجز"
    };
}
