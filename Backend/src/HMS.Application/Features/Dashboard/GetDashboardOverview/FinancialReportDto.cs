using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HMS.Application.Features.Dashboard.GetDashboardOverview
{
    public class FinancialReportDto
    {
        public decimal CashRevenue { get; set; }
        public decimal InsuranceRevenue { get; set; }
        public decimal ReferralRevenue { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}
