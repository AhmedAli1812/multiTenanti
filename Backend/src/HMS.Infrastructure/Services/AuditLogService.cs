
using HMS.Domain.Entities.Audit;
using HMS.Infrastructure.Persistence;
using System.Text.Json;

namespace HMS.Infrastructure.Services
{
    public class AuditLogService
    {
        private readonly ApplicationDbContext _context;

        public AuditLogService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task LogAsync(
            Guid? tenantId,            // 🔥 nullable
            Guid? userId,
            string? userName,
            string action,
            string entityName,
            string entityId,
            object? oldValues,
            object? newValues,
            string? ip)
        {
            var log = new AuditLog
            {
                Id = Guid.NewGuid(),

                // 🔥 tenant nullable (Super Admin / Login)
                TenantId = tenantId,

                UserId = userId,
                UserName = userName ?? "Anonymous",

                Action = action,
                EntityName = entityName,
                EntityId = entityId,

                OldValues = oldValues != null
                    ? JsonSerializer.Serialize(oldValues)
                    : null,

                NewValues = newValues != null
                    ? JsonSerializer.Serialize(newValues)
                    : null,

                IPAddress = ip,
                CreatedAt = DateTime.UtcNow
            };

            await _context.AuditLogs.AddAsync(log);
            await _context.SaveChangesAsync();
        }
    }
}

