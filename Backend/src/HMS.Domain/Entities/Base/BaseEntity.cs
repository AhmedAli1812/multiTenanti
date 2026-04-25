using HMS.Domain.Interfaces;

namespace HMS.Domain.Entities.Base;

public abstract class BaseEntity : ISoftDeletable
{
    public Guid Id { get; set; } = Guid.NewGuid();

    // =========================
    // 🧠 AUDIT
    // =========================
    public DateTime CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; } // 🔥 لازم تبقى nullable
    public Guid? UpdatedBy { get; set; }

    // =========================
    // 🗑️ SOFT DELETE
    // =========================
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }
}