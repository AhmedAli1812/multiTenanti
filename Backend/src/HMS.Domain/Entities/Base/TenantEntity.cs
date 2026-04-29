namespace HMS.Domain.Entities.Base;

public abstract class TenantEntity : BaseEntity, ITenantEntity
{
    public Guid? TenantId { get; set; }
}