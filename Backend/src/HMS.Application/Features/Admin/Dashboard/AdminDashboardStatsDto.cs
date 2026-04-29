namespace HMS.Application.Features.Admin.Dashboard;

public class AdminDashboardStatsDto
{
    public SummaryDto Summary { get; set; } = new();
    public List<MonthlyChartDto> MonthlyChart { get; set; } = [];
    public List<PayerDistributionDto> PayerDistribution { get; set; } = [];
    public List<DoctorPerformanceDto> TopDoctors { get; set; } = [];
    public List<SpecialtyStatsDto> Specialties { get; set; } = [];
}

public class SummaryDto
{
    public int TotalCases { get; set; }
    public double GrowthPercentage { get; set; }
    public int CashCases { get; set; }
    public double CashGrowth { get; set; }
    public int ContractCases { get; set; }
    public double ContractGrowth { get; set; }
    public int ReferralCases { get; set; }
    public double ReferralGrowth { get; set; }
}

public class MonthlyChartDto
{
    public string Month { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class PayerDistributionDto
{
    public string Name { get; set; } = string.Empty;
    public int Value { get; set; }
    public string Color { get; set; } = string.Empty;
}

public class DoctorPerformanceDto
{
    public string DoctorName { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public int TotalCases { get; set; }
    public int CashCases { get; set; }
    public int ContractCases { get; set; }
    public int LastMonthCases { get; set; }
    public int Diff { get; set; }
    public string Status { get; set; } = "Unchanged"; // Increased, Decreased, Unchanged
}

public class SpecialtyStatsDto
{
    public string Specialty { get; set; } = string.Empty;
    public int Count { get; set; }
}
