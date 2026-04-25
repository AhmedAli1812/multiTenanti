using HMS.Application.Abstractions.Persistence;
using HMS.Application.Abstractions.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HMS.Application.Features.Patients.Update;

public class UpdatePatientHandler : IRequestHandler<UpdatePatientCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public UpdatePatientHandler(
        IApplicationDbContext context,
        ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task Handle(UpdatePatientCommand request, CancellationToken cancellationToken)
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

        if (patient == null || patient.IsDeleted)
            throw new InvalidOperationException("Patient not found");

        // =========================
        // 💣 Validation
        // =========================
        if (string.IsNullOrWhiteSpace(request.FullName))
            throw new ArgumentException("Full name is required");

        if (string.IsNullOrWhiteSpace(request.PhoneNumber))
            throw new ArgumentException("Phone number is required");

        var fullName = request.FullName.Trim();
        var phone = request.PhoneNumber.Trim();

        // =========================
        // 🔥 Prevent duplicate phone
        // =========================
        var phoneExists = await _context.Patients
            .AsNoTracking()
            .AnyAsync(x =>
                x.PhoneNumber == phone &&
                x.Id != request.Id &&
                x.TenantId == tenantId,
                cancellationToken);

        if (phoneExists)
            throw new InvalidOperationException("Phone number already in use");

        // =========================
        // 🧠 Update
        // =========================
        patient.FullName = fullName;
        patient.PhoneNumber = phone;
        patient.Address = request.Address?.Trim();

        // 🔐 Audit
        patient.UpdatedAt = DateTime.UtcNow;
        patient.UpdatedBy = _currentUser.UserId;

        // =========================
        // 💾 Save
        // =========================
        await _context.SaveChangesAsync(cancellationToken);
    }
}