using HMS.Domain.Entities.Identity;
using Microsoft.EntityFrameworkCore;

namespace HMS.Infrastructure.Persistence.Seed;

public static class DbSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        // =====================
        // Permissions
        // =====================
        if (!context.Permissions.Any(p => p.Code == "dashboard.view"))
        {
            var dashboardPermission = new Permission
            {
                Id = Guid.NewGuid(),
                Code = "dashboard.view",
                Module = "Dashboard",
                Action = "view",
                Description = "View dashboard"
            };

            await context.Permissions.AddAsync(dashboardPermission);
            await context.SaveChangesAsync();
        }

        // =====================
        // Roles
        // =====================
        var adminRole = await context.Roles
            .FirstOrDefaultAsync(r => r.Name == "Admin");

        if (adminRole == null)
        {
            adminRole = new Role
            {
                Id = Guid.NewGuid(),
                Name = "Admin",
                IsSystem = true
            };

            await context.Roles.AddAsync(adminRole);
            await context.SaveChangesAsync();
        }

        // =====================
        // Link Role + Permission
        // =====================
        var permission = await context.Permissions
            .FirstAsync(p => p.Code == "dashboard.view");

        var exists = await context.RolePermissions.AnyAsync(rp =>
            rp.RoleId == adminRole.Id &&
            rp.PermissionId == permission.Id);

        if (!exists)
        {
            await context.RolePermissions.AddAsync(new RolePermission
            {
                RoleId = adminRole.Id,
                PermissionId = permission.Id
            });

            await context.SaveChangesAsync();
        }
    }
}
