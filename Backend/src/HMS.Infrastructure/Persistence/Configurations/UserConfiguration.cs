using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HMS.Domain.Entities.Identity;

namespace HMS.Infrastructure.Persistence.Configurations;

public class UserConfiguration : TenantEntityConfiguration<User>
{
    public override void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        base.Configure(builder);

        builder.Property(x => x.FullName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Email)
            .HasMaxLength(200);

        builder.Property(x => x.PhoneNumber)
            .HasMaxLength(50);

        builder.HasIndex(x => new { x.Email, x.TenantId })
            .IsUnique()
            .HasFilter("[Email] IS NOT NULL");

        builder.HasIndex(x => new { x.PhoneNumber, x.TenantId })
            .IsUnique()
            .HasFilter("[PhoneNumber] IS NOT NULL");
    }
}