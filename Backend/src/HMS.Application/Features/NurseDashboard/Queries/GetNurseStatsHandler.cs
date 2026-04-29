using HMS.Application.Abstractions.Persistence;
using HMS.Application.Features.NurseDashboard.Dtos;
using HMS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HMS.Application.Features.NurseDashboard.Queries;

public class GetNurseStatsHandler : IRequestHandler<GetNurseStatsQuery, NurseStatsDto>
{
    private readonly IApplicationDbContext _context;

    public GetNurseStatsHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<NurseStatsDto> Handle(GetNurseStatsQuery request, CancellationToken ct)
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        var todayVisits = _context.Visits
            .AsNoTracking()
            .Where(v => v.TenantId == request.TenantId
                     && v.VisitDate >= today
                     && v.VisitDate < tomorrow);

        var totalPatientsToday = await todayVisits.CountAsync(ct);

        var waitingPatients = await todayVisits
            .CountAsync(v =>
                v.Status == VisitStatus.CheckedIn ||
                v.Status == VisitStatus.WaitingDoctor, ct);

        var now = DateTime.UtcNow;
        var upcomingAppointments = await todayVisits
            .CountAsync(v =>
                v.Status != VisitStatus.Completed &&
                v.VisitDate > now, ct);

        var emergencyCases = await todayVisits
            .CountAsync(v =>
                v.VisitType == VisitType.Emergency ||
                v.Priority == PriorityLevel.Emergency, ct);

        return new NurseStatsDto
        {
            TotalPatientsToday = totalPatientsToday,
            WaitingPatients = waitingPatients,
            UpcomingAppointments = upcomingAppointments,
            EmergencyCases = emergencyCases
        };
    }
}
