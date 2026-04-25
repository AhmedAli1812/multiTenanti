using HMS.Application.Abstractions.Persistence;
using HMS.Application.Abstractions.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HMS.Application.Features.Patients.Delete;

public class DeletePatientHandler : IRequestHandler<DeletePatientCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public DeletePatientHandler(
        IApplicationDbContext context,
        ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task Handle(DeletePatientCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _currentUser.TenantId;

        // =========================
        // 🔥 Get Patient (Tenant Safe)
        // =========================
        var patient = await _context.Patients
            .FirstOrDefaultAsync(x =>
                x.Id == request.Id &&
                x.TenantId == tenantId,
                cancellationToken);

        if (patient == null)
            throw new InvalidOperationException("Patient not found");

        // =========================
        // 💣 Already deleted
        // =========================
        if (patient.IsDeleted)
            return;

        // =========================
        // 🚫 Prevent delete if linked to visits
        // =========================
        var hasVisits = await _context.Visits
            .AsNoTracking()
            .AnyAsync(v =>
                v.PatientId == patient.Id &&
                v.TenantId == tenantId,
                cancellationToken);

        if (hasVisits)
            throw new InvalidOperationException("Cannot delete patient with existing visits");

        // =========================
        // 🧠 Soft Delete
        // =========================
        patient.IsDeleted = true;
        patient.DeletedAt = DateTime.UtcNow;

        // optional audit
        patient.DeletedBy = _currentUser.UserId;

        // =========================
        // 💾 Save
        // =========================
        await _context.SaveChangesAsync(cancellationToken);
    }
}