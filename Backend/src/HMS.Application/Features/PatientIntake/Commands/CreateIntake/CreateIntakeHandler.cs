using HMS.Application.Abstractions.Persistence;
using HMS.Application.Abstractions.CurrentUser;
using HMS.Domain.Entities.PatientIntake;
using HMS.Domain.Enums;
using MediatR;

public class CreateIntakeHandler : IRequestHandler<CreateIntakeCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public CreateIntakeHandler(
        IApplicationDbContext context,
        ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(CreateIntakeCommand request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;
        var userId = _currentUser.UserId;

        // =========================
        // 💣 Validation
        // =========================
        if (request.BranchId == Guid.Empty)
            throw new ArgumentException("Branch is required");

        // =========================
        // 🔥 Create Intake (Draft بدون Patient)
        // =========================
        var intake = new PatientIntake
        {
            Id = Guid.NewGuid(),
            BranchId = request.BranchId, // ✅ مباشر
            Status = IntakeStatus.Draft,
            TenantId = tenantId,

            PatientId = null, // 👈 لسه Draft

            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };

        await _context.Intakes.AddAsync(intake, ct);

        // =========================
        // 💾 Save
        // =========================
        await _context.SaveChangesAsync(ct);

        return intake.Id;
    }
}