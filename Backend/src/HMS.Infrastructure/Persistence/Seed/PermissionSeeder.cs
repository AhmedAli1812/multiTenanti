using HMS.Application.Abstractions.Persistence;
using HMS.Domain.Constants;
using HMS.Domain.Entities.Identity;
using Microsoft.EntityFrameworkCore;

namespace HMS.Infrastructure.Persistence.Seed;

public static class PermissionSeeder
{
    public static async Task SeedAsync(IApplicationDbContext context)
    {
        var permissions = new List<Permission>
        {
            // 🔥 Users
            new Permission { Code = "users.view", Module = "Users", Action = "view", Description = "View users" },
            new Permission { Code = "users.create", Module = "Users", Action = "create", Description = "Create users" },
            new Permission { Code = "users.delete", Module = "Users", Action = "delete", Description = "Delete users" },
            new Permission { Code = "users.assignRole", Module = "Users", Action = "assign-role", Description = "Assign role to user" },

            // 🔥 Visits
            new Permission { Code = "visits.view", Module = "Visits", Action = "view", Description = "View visits" },
            new Permission { Code = "visits.create", Module = "Visits", Action = "create", Description = "Create visits" },

            // 🔥 Doctors
            new Permission { Code = "doctors.queue", Module = "Doctors", Action = "view", Description = "View doctor queue" },
            new Permission { Code = "doctors.write-notes", Module = "Doctors", Action = "write", Description = "Write medical notes" },

            // 🔥 Dashboard
            new Permission { Code = "dashboard.view", Module = "Dashboard", Action = "view", Description = "View dashboard" },
            new Permission { Code = "dashboard.reception.view", Module = "Dashboard", Action = "view", Description = "View reception dashboard" },
            // 🔥 Audit Logs
            new Permission { Code = "audit_logs.view", Module = "Audit Logs", Action = "view", Description = "View audit logs" },
            
             new Permission
            {
                Id = Guid.NewGuid(),
                Code = Permissions.PatientsCreate,
                Module = "Patients",
                Action = "create",
                Description = "Create patient"
            }


        };

        foreach (var permission in permissions)
        {
            var exists = await context.Permissions
                .AnyAsync(p => p.Code == permission.Code);

            if (!exists)
            {
                permission.Id = Guid.NewGuid();
                await context.Permissions.AddAsync(permission);
            }
        }


        await context.SaveChangesAsync(CancellationToken.None);
    }
}