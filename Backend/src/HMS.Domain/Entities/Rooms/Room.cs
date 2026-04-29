using HMS.Domain.Entities.Base;
using HMS.Domain.Entities.Branches;
using HMS.Domain.Entities.Departments;
using HMS.Domain.Enums;

namespace HMS.Domain.Entities.Rooms;

public class Room : TenantEntity
{
    public Guid Id { get; set; }

    // 🔥 Display Name (مهم للـ Dashboard)
    public string Name { get; set; } = string.Empty;

    // 🔢 رقم الغرفة
    public string RoomNumber { get; set; } = string.Empty;

    public RoomType Type { get; set; }

    public int Capacity { get; set; } = 1;

    // 🏥 Branch
    public Guid BranchId { get; set; }
    public Branch Branch { get; set; } = default!;

// 🏢 Floor
public Guid FloorId { get; set; }
    public Floor Floor { get; set; } = default!;

    // ⚙️ الحالة
    public bool IsOccupied { get; private set; }

    public DateTime? CleaningUntil { get; private set; }

    // =========================
    // 💣 DOMAIN LOGIC
    // =========================

    public bool IsAvailable()
    {
        var now = DateTime.UtcNow;

        return !IsOccupied &&
               (CleaningUntil == null || CleaningUntil <= now);
    }

    public void Assign()
    {
        if (!IsAvailable())
            throw new InvalidOperationException("Room is not available");

        IsOccupied = true;
    }

    public void Release()
    {
        IsOccupied = false;
        CleaningUntil = DateTime.UtcNow.AddMinutes(15);
    }
    public string GetStatus()
    {
        var now = DateTime.UtcNow;

        if (CleaningUntil != null && CleaningUntil > now)
            return "Cleaning";

        if (IsOccupied)
            return "Occupied";

        return "Available";
    }
   
}