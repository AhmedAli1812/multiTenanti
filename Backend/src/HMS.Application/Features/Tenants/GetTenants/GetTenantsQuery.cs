using MediatR;

namespace HMS.Application.Features.Tenants.GetTenants;

public record GetTenantsQuery : IRequest<List<TenantDto>>;

public class TenantDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string Code { get; set; } = default!;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
