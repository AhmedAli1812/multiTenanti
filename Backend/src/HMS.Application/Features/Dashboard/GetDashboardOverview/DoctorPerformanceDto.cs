public class DoctorPerformanceDto
{
    public string DoctorName { get; set; }
    public int TotalCases { get; set; }
    public int CashCases { get; set; }
    public int InsuranceCases { get; set; }
    public int ReferralCases { get; set; }
    public int PreviousCases { get; set; } 
    public double ChangePercentage { get; set; } 
}