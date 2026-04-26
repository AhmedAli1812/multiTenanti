using HMS.Application.Abstractions.Persistence;
using HMS.Application.Abstractions.CurrentUser;
using HMS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

public class UpdateIntakeHandler : IRequestHandler<UpdateIntakeCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public UpdateIntakeHandler(
        IApplicationDbContext context,
        ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(UpdateIntakeCommand request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;
        var userId = _currentUser.UserId;

        // =========================
        // 🔥 Get Intake (Multi-tenant safe)
        // =========================
        var intakeQuery = _context.Intakes
            .Where(x => x.Id == request.IntakeId);

        if (!_currentUser.IsGlobal)
        {
            intakeQuery = intakeQuery.Where(x => x.TenantId == tenantId);
        }

        var intake = await intakeQuery.FirstOrDefaultAsync(ct);

        if (intake == null)
            throw new InvalidOperationException("Intake not found");

        // =========================
        // 💣 Business Rule
        // =========================
        if (intake.Status != IntakeStatus.Draft)
            throw new InvalidOperationException("Cannot update after submission");

        // =========================
        // 🔍 Validation
        // =========================
        if (request.BranchId.HasValue)
        {
            var branchQuery = _context.Branches
                .AsNoTracking()
                .Where(b => b.Id == request.BranchId.Value);

            if (!_currentUser.IsGlobal)
            {
                branchQuery = branchQuery.Where(b => b.TenantId == tenantId);
            }

            var branchExists = await branchQuery.AnyAsync(ct);

            if (!branchExists)
                throw new InvalidOperationException("Invalid branch");

            intake.BranchId = request.BranchId.Value;
        }

        // =========================
        // 🔄 Update fields
        // =========================
        if (request.VisitType.HasValue)
            intake.VisitType = request.VisitType.Value;

        if (request.Priority.HasValue)
            intake.Priority = request.Priority.Value;

        if (!string.IsNullOrWhiteSpace(request.ChiefComplaint))
            intake.ChiefComplaint = request.ChiefComplaint.Trim();

        // =========================
        // 🧾 JSON fields
        // =========================
        intake.EmergencyContactJson = request.EmergencyContactJson;
        intake.InsuranceJson = request.InsuranceJson;
        intake.FlagsJson = request.FlagsJson;

        // =========================
        // 🧾 Audit
        // =========================
        intake.UpdatedAt = DateTime.UtcNow;
        intake.UpdatedBy = userId;

        // =========================
        // 💾 Save
        // =========================
        await _context.SaveChangesAsync(ct);

        return Unit.Value;
    }
}