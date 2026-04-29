using HMS.Domain.Entities.Tenancy;
using Microsoft.EntityFrameworkCore;

namespace HMS.Infrastructure.Persistence.Seed;

public static class TenantSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context, CancellationToken cancellationToken)
    {
        if (await context.Tenants.AnyAsync(cancellationToken))
            return;

        var tenants = new List<Tenant>
        {
            new() { Name = "MedScope Hospital", Code = "MED", IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { Name = "El Hayat Hospital", Code = "ELH", IsActive = true, CreatedAt = DateTime.UtcNow }
        };

        await context.Tenants.AddRangeAsync(tenants, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }
}
