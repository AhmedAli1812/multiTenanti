using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HMS.Domain.Entities.Base;

namespace HMS.Infrastructure.Persistence.Configurations;

public abstract class BaseEntityConfiguration<T> : IEntityTypeConfiguration<T>
    where T : BaseEntity
{
    public virtual void Configure(EntityTypeBuilder<T> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        // UpdatedAt is DateTime? in BaseEntity — must be nullable in DB schema.
        // BUG (before fix): Was configured as IsRequired() which caused a
        // schema/model mismatch — EF generated a NOT NULL column while the
        // CLR property was nullable, causing insert failures when UpdatedAt
        // was not set at creation time.
        builder.Property(x => x.UpdatedAt)
            .IsRequired(false);

        builder.Property(x => x.IsDeleted)
            .HasDefaultValue(false);
    }
}