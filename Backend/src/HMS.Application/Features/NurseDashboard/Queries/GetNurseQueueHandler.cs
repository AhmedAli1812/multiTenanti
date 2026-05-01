using HMS.Application.Abstractions.Persistence;
using HMS.Application.Features.NurseDashboard.Dtos;
using HMS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HMS.Application.Features.NurseDashboard.Queries;

public class GetNurseQueueHandler : IRequestHandler<GetNurseQueueQuery, List<QueuePatientDto>>
{
    private readonly IApplicationDbContext _context;

    public GetNurseQueueHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<QueuePatientDto>> Handle(GetNurseQueueQuery request, CancellationToken ct)
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        var visits = await _context.Visits
            .AsNoTracking()
            .Where(v => v.TenantId == request.TenantId
                     && v.VisitDate >= today
                     && v.VisitDate < tomorrow
                     && v.Status != VisitStatus.Completed)
            .OrderByDescending(v => v.Priority)
            .ThenBy(v => v.VisitDate)
            .Select(v => new
            {
                v.Id,
                v.VisitDate,
                v.VisitType,
                v.Status,
                v.Priority,
                v.QueueNumber,
                PatientName = v.Patient != null ? v.Patient.FullName : "-",
                NationalId = v.Patient != null ? (v.Patient.NationalId ?? v.Patient.MedicalNumber) : "-",
                DoctorName = v.Doctor != null ? v.Doctor.FullName : "-",
                DepartmentName = v.Doctor != null && v.Doctor.Department != null
                    ? v.Doctor.Department.Name : "-",
                v.ChiefComplaint,
                v.Notes
            })
            .ToListAsync(ct);

        return visits.Select(v => new QueuePatientDto
        {
            VisitId = v.Id,
            PatientName = v.PatientName ?? "",
            NationalId = v.NationalId ?? "",
            ArrivalTime = v.VisitDate,
            VisitTypeName = MapVisitType(v.VisitType),
            StatusName = MapStatus(v.Status),
            DoctorName = v.DoctorName ?? "-",
            DepartmentName = v.DepartmentName ?? "-",
            PriorityName = MapPriority(v.Priority),
            ChiefComplaint = v.ChiefComplaint,
            Notes = v.Notes,
            QueueNumber = v.QueueNumber
        }).ToList();
    }

    private static string MapVisitType(VisitType type) => type switch
    {
        VisitType.Outpatient => "حجز",
        VisitType.Emergency  => "طارئ",
        VisitType.Inpatient  => "راجع",
        _                    => "حجز"
    };

    private static string MapStatus(VisitStatus status) => status switch
    {
        VisitStatus.CheckedIn     => "تم الدخول",
        VisitStatus.WaitingDoctor => "انتظار الطبيب",
        VisitStatus.Prepared      => "جاهز",
        VisitStatus.InOp          => "قيد العملية",
        VisitStatus.OpCompleted   => "انتهت العملية",
        VisitStatus.PostOp        => "ما بعد العملية",
        VisitStatus.Completed     => "مكتمل",
        _                         => "نشط"
    };

    private static string MapPriority(PriorityLevel priority) => priority switch
    {
        PriorityLevel.Normal    => "عادي",
        PriorityLevel.Urgent    => "عاجل",
        PriorityLevel.Emergency => "طوارئ",
        _                       => "عادي"
    };
}
