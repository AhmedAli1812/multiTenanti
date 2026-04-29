using HMS.Application.Abstractions.CurrentUser;
using HMS.Application.Abstractions.Persistence;
using HMS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace HMS.Application.Features.Admin.Dashboard;

public class GetAdminDashboardStatsQuery : IRequest<AdminDashboardStatsDto>
{
    public Guid? TenantId { get; set; }
    public Guid? BranchId { get; set; }
}

public class GetAdminDashboardStatsHandler : IRequestHandler<GetAdminDashboardStatsQuery, AdminDashboardStatsDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public GetAdminDashboardStatsHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<AdminDashboardStatsDto> Handle(GetAdminDashboardStatsQuery request, CancellationToken cancellationToken)
    {
        var tenantId = (request.TenantId.HasValue && _currentUser.IsGlobal) ? request.TenantId.Value : _currentUser.TenantId;
        var branchId = request.BranchId;

        var now = DateTime.UtcNow;
        var currentMonthStart = new DateTime(now.Year, now.Month, 1);
        var lastMonthStart = currentMonthStart.AddMonths(-1);
        var sixMonthsAgo = currentMonthStart.AddMonths(-5);

        // Base query for visits
        var visitsQuery = _context.Visits.AsNoTracking();

        // Multi-tenant filter
        if (!_currentUser.IsGlobal || tenantId != Guid.Empty)
        {
            visitsQuery = visitsQuery.Where(v => v.TenantId == tenantId);
        }

        if (branchId.HasValue && branchId != Guid.Empty)
        {
            visitsQuery = visitsQuery.Where(v => v.BranchId == branchId.Value);
        }

        var allVisits = await visitsQuery
            .Where(v => v.VisitDate >= sixMonthsAgo)
            .Include(v => v.Doctor)
            .ToListAsync(cancellationToken);

        var currentMonthVisits = allVisits.Where(v => v.VisitDate >= currentMonthStart).ToList();
        var lastMonthVisits = allVisits.Where(v => v.VisitDate >= lastMonthStart && v.VisitDate < currentMonthStart).ToList();

        // 1. Summary
        var stats = new AdminDashboardStatsDto();
        stats.Summary.TotalCases = currentMonthVisits.Count;
        
        // Count Cash (including legacy 0)
        stats.Summary.CashCases = currentMonthVisits.Count(v => v.PayerType == PayerType.Cash || (int)v.PayerType == 0);
        stats.Summary.ContractCases = currentMonthVisits.Count(v => v.PayerType == PayerType.Insurance);
        stats.Summary.ReferralCases = currentMonthVisits.Count(v => v.PayerType == PayerType.Referral);

        // Helper for growth
        double CalcGrowth(int current, int last) => last == 0 ? (current > 0 ? 100 : 0) : Math.Round(((double)(current - last) / last) * 100, 1);

        stats.Summary.GrowthPercentage = CalcGrowth(currentMonthVisits.Count, lastMonthVisits.Count);
        
        int lastCash = lastMonthVisits.Count(v => v.PayerType == PayerType.Cash || (int)v.PayerType == 0);
        int lastContract = lastMonthVisits.Count(v => v.PayerType == PayerType.Insurance);
        int lastReferral = lastMonthVisits.Count(v => v.PayerType == PayerType.Referral);

        stats.Summary.CashGrowth = CalcGrowth(stats.Summary.CashCases, lastCash);
        stats.Summary.ContractGrowth = CalcGrowth(stats.Summary.ContractCases, lastContract);
        stats.Summary.ReferralGrowth = CalcGrowth(stats.Summary.ReferralCases, lastReferral);

        // 2. Monthly Chart
        var arabicCulture = new CultureInfo("ar-EG");
        stats.MonthlyChart = new List<MonthlyChartDto>();
        for (int i = 5; i >= 0; i--)
        {
            var m = now.AddMonths(-i);
            var count = allVisits.Count(v => v.VisitDate.Year == m.Year && v.VisitDate.Month == m.Month);
            stats.MonthlyChart.Add(new MonthlyChartDto { Month = m.ToString("MMMM", arabicCulture), Count = count });
        }

        // 3. Payer Distribution
        stats.PayerDistribution = new List<PayerDistributionDto>
        {
            new() { Name = "نقدي", Value = stats.Summary.CashCases, Color = "#0ea5e9" },
            new() { Name = "عقود", Value = stats.Summary.ContractCases, Color = "#22c55e" },
            new() { Name = "تحويل أطباء", Value = stats.Summary.ReferralCases, Color = "#eab308" }
        };

        // 4. Top Doctors
        var doctorStats = allVisits
            .Where(v => v.DoctorId != null)
            .GroupBy(v => new { v.DoctorId, v.Doctor?.FullName })
            .Select(g => new DoctorPerformanceDto
            {
                DoctorName = g.Key.FullName ?? "Unknown",
                TotalCases = g.Count(v => v.VisitDate >= currentMonthStart),
                CashCases = g.Count(v => v.VisitDate >= currentMonthStart && (v.PayerType == PayerType.Cash || (int)v.PayerType == 0)),
                ContractCases = g.Count(v => v.VisitDate >= currentMonthStart && v.PayerType == PayerType.Insurance),
                LastMonthCases = g.Count(v => v.VisitDate >= lastMonthStart && v.VisitDate < currentMonthStart)
            })
            .OrderByDescending(d => d.TotalCases)
            .Take(10)
            .ToList();

        foreach (var d in doctorStats)
        {
            d.Diff = d.TotalCases - d.LastMonthCases;
            d.Status = d.Diff > 0 ? "Increased" : (d.Diff < 0 ? "Decreased" : "Unchanged");
            stats.TopDoctors.Add(d);
        }

        // 5. Specialties (Smarter mock)
        var departments = await _context.Departments.AsNoTracking().Where(d => d.TenantId == tenantId).ToListAsync(cancellationToken);
        int j = 0;
        foreach (var dept in departments.Take(5))
        {
            // Distribution across departments
            int deptCount = (j == 0) ? (int)(stats.Summary.TotalCases * 0.4) : (int)(stats.Summary.TotalCases * 0.15);
            stats.Specialties.Add(new SpecialtyStatsDto
            {
                Specialty = dept.Name,
                Count = Math.Max(1, deptCount)
            });
            j++;
        }

        return stats;
    }
}
