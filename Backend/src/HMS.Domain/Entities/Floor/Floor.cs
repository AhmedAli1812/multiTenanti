using HMS.Domain.Entities.Branches;
using HMS.Domain.Entities.Base;
using HMS.Domain.Entities.Rooms;
public class Floor: TenantEntity
{
    public Guid Id { get; set; }

    public string Name { get; set; } = default!; // مثال: "الدور الأول"

    public int Number { get; set; } // 1,2,3...

    // 🔗 Relation
    public Guid BranchId { get; set; }
    public Branch Branch { get; set; } = default!;

    public ICollection<Room> Rooms { get; set; } = new List<Room>();
}