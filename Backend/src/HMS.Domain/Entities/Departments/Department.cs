namespace HMS.Domain.Entities.Departments;

using HMS.Domain.Entities.Base;
using HMS.Domain.Entities.Branches;

public class Department : TenantEntity
{
    public Guid Id { get; set; }

    public string Name { get; set; } = default!;

    public Guid BranchId { get; set; }
    public Branch Branch { get; set; } = default!;
}