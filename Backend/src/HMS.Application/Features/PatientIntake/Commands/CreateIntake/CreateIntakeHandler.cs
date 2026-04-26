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
        // 🔥 Get Patient + TenantId
        // =========================
        var patientData = await _context.Patients
            .AsNoTracking()
            .Where(p => p.Id == request.PatientId && !p.IsDeleted)
            .Select(p => new
            {
                p.Id,
                p.TenantId
            })
            .FirstOrDefaultAsync(ct);

        if (patientData == null)
            throw new InvalidOperationException("Patient not found");

        // =========================
        // 🔥 Multi-Tenant Check
        // =========================
        if (!_currentUser.IsGlobal && patientData.TenantId != tenantId)
            throw new UnauthorizedAccessException("Access denied");

        // =========================
        // 🔥 Prevent duplicate intake
        // =========================
        var intakeQuery = _context.Intakes
            .AsNoTracking()
            .Where(i =>
                i.PatientId == request.PatientId &&
                i.Status == IntakeStatus.Draft);

        if (!_currentUser.IsGlobal)
        {
            intakeQuery = intakeQuery.Where(i => i.TenantId == tenantId);
        }

        var hasActiveIntake = await intakeQuery.AnyAsync(ct);

        if (hasActiveIntake)
            throw new InvalidOperationException("Patient already has an active intake");

        // =========================
        // 🔥 Resolve TenantId (FIX)
        // =========================
        if (patientData.TenantId == null)
            throw new InvalidOperationException("Patient TenantId is missing");

        var finalTenantId = _currentUser.IsGlobal
            ? patientData.TenantId.Value
            : tenantId;

        // =========================
        // 🔥 Create Intake
        // =========================
        var intake = new PatientIntake
        {
            Id = Guid.NewGuid(),
            PatientId = request.PatientId,
            BranchId = request.BranchId.Value,
            Status = IntakeStatus.Draft,
            TenantId = finalTenantId,

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