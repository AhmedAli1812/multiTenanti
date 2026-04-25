using HMS.Domain.Entities.Base;

namespace HMS.Domain.Entities
{
    public class Notification : TenantEntity
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public string Title { get; set; } = default!;
        public string Message { get; set; } = default!;

        public string Type { get; set; } = "info"; // info, warning, critical

        public bool IsRead { get; set; } = false;

        public string? ReferenceType { get; set; } // Visit / Room
        public Guid? ReferenceId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}