using HMS.Domain.Entities.Identity;
using Microsoft.EntityFrameworkCore;

namespace HMS.Infrastructure.Persistence.Seeds;

public static class SystemRolesSeeder
{
    private static readonly Guid SUPER_ADMIN_ROLE_ID = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private static readonly Guid ADMIN_ROLE_ID = Guid.Parse("00000000-0000-0000-0000-000000000002");
    private static readonly Guid DOCTOR_ROLE_ID = Guid.Parse("00000000-0000-0000-0000-000000000003");
    private static readonly Guid PATIENT_ROLE_ID = Guid.Parse("00000000-0000-0000-0000-000000000004");

    public static async Task SeedAsync(ApplicationDbContext context, CancellationToken cancellationToken)
    {
        // Check if roles already exist (prevent duplicates)
        if (await context.Roles.AnyAsync(cancellationToken))
            return;

        var systemRoles = new List<Role>
        {
            new() { Id = SUPER_ADMIN_ROLE_ID, Name = "SuperAdmin", IsSystem = true },
            new() { Id = ADMIN_ROLE_ID, Name = "Admin", IsSystem = true },
            new() { Id = DOCTOR_ROLE_ID, Name = "Doctor", IsSystem = true },
            new() { Id = PATIENT_ROLE_ID, Name = "Patient", IsSystem = true }
        };

        await context.Roles.AddRangeAsync(systemRoles, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }
}