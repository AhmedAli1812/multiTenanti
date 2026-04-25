using MediatR;

public class GetDashboardOverviewQuery : IRequest<DashboardResponse>
{
    public int? Month { get; set; }
    public int? Year { get; set; }
    public Guid? DoctorId { get; set; }
    public Guid? BranchId { get; set; }
}