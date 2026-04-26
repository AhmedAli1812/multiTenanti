using MediatR;

namespace HMS.Application.Features.Reception.Departments;

public class GetDepartmentsQuery : IRequest<List<DepartmentDto>>
{
    public Guid? BranchId { get; set; } // 🔥 filter
}