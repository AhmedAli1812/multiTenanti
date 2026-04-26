using HMS.Application.Features.Dashboard.GetDashboardOverview;

public class DashboardResponse
{
    public KpiDto Kpis { get; set; }
    public CaseDistributionDto Distribution { get; set; }
    public List<MonthlyTrendDto> MonthlyTrend { get; set; }
    public List<DoctorPerformanceDto> Doctors { get; set; }
    public List<TopDoctorDto> TopDoctors { get; set; } 
    public List<AlertDto> Alerts { get; set; } 
}