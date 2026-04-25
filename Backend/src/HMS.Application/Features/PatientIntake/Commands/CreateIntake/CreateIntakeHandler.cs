using HMS.Application.Abstractions.Persistence;
using HMS.Application.Abstractions.CurrentUser;
using HMS.Domain.Entities.PatientIntake;
using HMS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

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
        if (request.PatientId == Guid.Empty)
            throw new ArgumentException("Patient is required");

        if (!request.BranchId.HasValue || request.BranchId.Value == Guid.Empty)
            throw new ArgumentException("Branch is required");

        // =========================
        // 🔥 Check Patient exists
        // =========================
        var patientExists = await _context.Patients
            .AsNoTracking()
            .AnyAsync(p =>
                p.Id == request.PatientId &&
                p.TenantId == tenantId,
                ct);

        if (!patientExists)
            throw new InvalidOperationException("Patient not found");

        // =========================
        // 🔥 Prevent duplicate active intake
        // =========================
        var hasActiveIntake = await _context.Intakes
            .AsNoTracking()
            .AnyAsync(i =>
                i.PatientId == request.PatientId &&
                i.Status == IntakeStatus.Draft &&
                i.TenantId == tenantId,
                ct);

        if (hasActiveIntake)
            throw new InvalidOperationException("Patient already has an active intake");

        // =========================
        // 🔥 Create Intake
        // =========================
        var intake = new PatientIntake
        {
            Id = Guid.NewGuid(),
            PatientId = request.PatientId,
            BranchId = request.BranchId.Value, // ✅ حل nullable
            Status = IntakeStatus.Draft,
            TenantId = tenantId,

            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId // 🔥 audit
        };

        await _context.Intakes.AddAsync(intake, ct);

        // =========================
        // 💾 Save
        // =========================
        await _context.SaveChangesAsync(ct);

        return intake.Id;
    }
}