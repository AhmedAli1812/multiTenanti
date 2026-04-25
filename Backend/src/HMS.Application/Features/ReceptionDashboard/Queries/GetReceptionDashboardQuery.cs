using HMS.Application.Features.ReceptionDashboard.Dtos;
using MediatR;

public class GetReceptionDashboardQuery : IRequest<ReceptionDashboardDto>
{
    public Guid TenantId { get; set; }

    // 🔍 Filters
    public string? Search { get; set; }
    public Guid? DepartmentId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }

    // 📄 Pagination
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}