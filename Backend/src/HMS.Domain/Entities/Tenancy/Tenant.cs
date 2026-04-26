namespace HMS.Domain.Entities.Tenancy;

public class Tenant
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = default!;

    public string Code { get; set; } = default!; // used in login

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }
}