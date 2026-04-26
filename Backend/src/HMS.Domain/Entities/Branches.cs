using HMS.Domain.Entities.Base;
using HMS.Domain.Entities.Tenancy;
using System.Drawing;

namespace HMS.Domain.Entities.Branches;

public class Branch : TenantEntity
{
    public Guid Id { get; set; }

    public string Name { get; set; } = default!;
    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = default!;
    public string? Address { get; set; }

    public string? Phone { get; set; }
    public ICollection<Floor> Floors { get; set; } = new List<Floor>();
}