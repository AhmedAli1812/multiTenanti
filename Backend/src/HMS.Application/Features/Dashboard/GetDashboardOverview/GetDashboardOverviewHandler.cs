using HMS.Application.Abstractions.CurrentUser;
using HMS.Application.Abstractions.Persistence;
using HMS.Application.Features.Dashboard.GetDashboardOverview;
using HMS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

public class GetDashboardOverviewHandler
    : IRequestHandler<GetDashboardOverviewQuery, DashboardResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public GetDashboardOverviewHandler(
        IApplicationDbContext context,
        ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<DashboardResponse> Handle(
        GetDashboardOverviewQuery request,
        CancellationToken cancellationToken)
    {
        var tenantId = _currentUser.TenantId;

        var month = request.Month is >= 1 and <= 12 ? request.Month.Value : DateTime.UtcNow.Month;
        var year = request.Year > 2000 ? request.Year.Value : DateTime.UtcNow.Year;

        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1);

        var previousStart = startDate.AddMonths(-1);
        var previousEnd = startDate;

        var daysInMonth = DateTime.DaysInMonth(year, month);

        // =============================
        // 🔥 BASE QUERY (No Tracking)
        // =============================
        var baseQuery = _context.Visits
            .AsNoTracking()
            .Where(v => v.TenantId == tenantId &&
                        v.CreatedAt >= startDate &&
                        v.CreatedAt < endDate);

        // =============================
        // 🔥 KPIs (DB Level)
        // =============================
        var total = await baseQuery.CountAsync(cancellationToken);

        var cash = await baseQuery.CountAsync(v => v.PayerType == PayerType.Cash, cancellationToken);
        var insurance = await baseQuery.CountAsync(v => v.PayerType == PayerType.Insurance, cancellationToken);
        var referral = await baseQuery.CountAsync(v => v.PayerType == PayerType.Referral, cancellationToken);

        var distribution = new CaseDistributionDto
        {
            CashPercentage = total == 0 ? 0 : (double)cash / total * 100,
            InsurancePercentage = total == 0 ? 0 : (double)insurance / total * 100,
            ReferralPercentage = total == 0 ? 0 : (double)referral / total * 100
        };

        // =============================
        // 🔥 Growth
        // =============================
        var previousVisits = await _context.Visits
            .AsNoTracking()
            .CountAsync(v =>
                v.TenantId == tenantId &&
                v.CreatedAt >= previousStart &&
                v.CreatedAt < previousEnd,
                cancellationToken);

        var growthRate = previousVisits > 0
            ? ((double)(total - previousVisits) / previousVisits) * 100
            : 0;

        // =============================
        // 🔥 Monthly Trend (ONE QUERY)
        // =============================
        var monthlyTrend = await _context.Visits
            .AsNoTracking()
            .Where(v => v.TenantId == tenantId &&
                        v.CreatedAt >= startDate.AddMonths(-5) &&
                        v.CreatedAt < endDate)
            .GroupBy(v => new { v.CreatedAt.Year, v.CreatedAt.Month })
            .Select(g => new MonthlyTrendDto
            {
                Month = g.Key.Month.ToString(),
                TotalCases = g.Count()
            })
            .OrderBy(x => x.Month)
            .ToListAsync(cancellationToken);

        // =============================
        // 🔥 Doctor Performance (DB)
        // =============================
        var doctors = await _context.Visits
            .AsNoTracking()
            .Where(v => v.TenantId == tenantId &&
                        v.CreatedAt >= startDate &&
                        v.CreatedAt < endDate &&
                        v.DoctorId != null)
            .GroupBy(v => new { v.DoctorId, v.Doctor!.FullName })
            .Select(g => new DoctorPerformanceDto
            {
                DoctorName = g.Key.FullName,
                TotalCases = g.Count(),
                CashCases = g.Count(x => x.PayerType == PayerType.Cash),
                InsuranceCases = g.Count(x => x.PayerType == PayerType.Insurance),
                ReferralCases = g.Count(x => x.PayerType == PayerType.Referral)
            })
            .ToListAsync(cancellationToken);

        var topDoctors = doctors
            .OrderByDescending(d => d.TotalCases)
            .Take(10)
            .Select(d => new TopDoctorDto
            {
                DoctorName = d.DoctorName,
                TotalCases = d.TotalCases
            })
            .ToList();

        var alerts = doctors
            .Where(d => d.TotalCases == 0)
            .Select(d => new AlertDto
            {
                Message = $"Doctor {d.DoctorName} has no cases this month"
            })
            .ToList();

        // =============================
        // 🔥 Response
        // =============================
        return new DashboardResponse
        {
            Kpis = new KpiDto
            {
                TotalCases = total,
                CashCases = cash,
                InsuranceCases = insurance,
                ReferralCases = referral,
                GrowthRate = Math.Round(growthRate, 2)
            },
            Distribution = distribution,
            MonthlyTrend = monthlyTrend,
            Doctors = doctors,
            TopDoctors = topDoctors,
            Alerts = alerts
        };
    }
}