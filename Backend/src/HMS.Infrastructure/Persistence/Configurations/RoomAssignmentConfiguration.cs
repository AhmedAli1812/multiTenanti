using HMS.Domain.Entities.Operations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class RoomAssignmentConfiguration : IEntityTypeConfiguration<RoomAssignment>
{
    public void Configure(EntityTypeBuilder<RoomAssignment> builder)
    {
        builder.ToTable("RoomAssignments");
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.RoomId);
        builder.HasIndex(x => x.VisitId);
        builder.HasIndex(x => x.IsActive);
    }
}