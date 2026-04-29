using MediatR;

namespace HMS.Application.Features.Reception.Departments;

public class CreateDepartmentCommand : IRequest<Guid>
{
    public string Name { get; set; } = default!;
    public Guid BranchId { get; set; }
    public Guid? TenantId { get; set; }
}
