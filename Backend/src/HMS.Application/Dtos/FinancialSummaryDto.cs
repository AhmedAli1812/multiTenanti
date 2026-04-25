public class FinancialSummaryDto
{
    public decimal TotalRevenue { get; set; }
    public decimal CashRevenue { get; set; }
    public decimal InsuranceRevenue { get; set; }
    public decimal ReferralRevenue { get; set; }

    public List<MonthlyRevenueDto> MonthlyTrend { get; set; }
}

public class MonthlyRevenueDto
{
    public string Month { get; set; }
    public decimal Revenue { get; set; }
}