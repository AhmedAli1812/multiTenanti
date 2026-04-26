using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HMS.Domain.Entities.Base;

namespace HMS.Infrastructure.Persistence.Configurations;

public abstract class TenantEntityConfiguration<T> : BaseEntityConfiguration<T>
    where T : TenantEntity
{
    public override void Configure(EntityTypeBuilder<T> builder)
    {
        base.Configure(builder);

        // ✅ FIX
        builder.Property(x => x.TenantId)
            .IsRequired(false);

        builder.HasIndex(x => x.TenantId);
    }
}