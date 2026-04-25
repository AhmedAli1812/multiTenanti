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

        // =========================
        // 🔥 Get Intake (Tenant Safe)
        // =========================
        var intake = await _context.Intakes
            .FirstOrDefaultAsync(x =>
                x.Id == request.IntakeId &&
                x.TenantId == tenantId,
                ct);

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
            var branchExists = await _context.Branches
                .AsNoTracking()
                .AnyAsync(b =>
                    b.Id == request.BranchId.Value &&
                    b.TenantId == tenantId,
                    ct);

            if (!branchExists)
                throw new InvalidOperationException("Invalid branch");

            intake.BranchId = request.BranchId.Value;
        }

        // 🚫 مفيش Room هنا خلاص

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
        // 💾 Save
        // =========================
        await _context.SaveChangesAsync(ct);

        return Unit.Value;
    }
}