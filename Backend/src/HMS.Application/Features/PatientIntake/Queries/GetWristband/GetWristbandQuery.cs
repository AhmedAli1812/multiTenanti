using HMS.Application.Abstractions.CurrentUser;
using HMS.Application.Abstractions.Persistence;
using HMS.Application.Abstractions.Services;
using HMS.Application.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HMS.Application.Features.PatientIntake.Queries.GetWristband;

public class GetWristbandQuery : IRequest<WristbandDto>
{
    public Guid IntakeId { get; set; }
}

public class GetWristbandHandler : IRequestHandler<GetWristbandQuery, WristbandDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IQrCodeService _qr;
    private readonly ICurrentUser _currentUser;

    public GetWristbandHandler(
        IApplicationDbContext context,
        IQrCodeService qr,
        ICurrentUser currentUser)
    {
        _context = context;
        _qr = qr;
        _currentUser = currentUser;
    }

    public async Task<WristbandDto> Handle(GetWristbandQuery request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;

        // 1. Get Intake
        var intake = await _context.Intakes
            .FirstOrDefaultAsync(x => x.Id == request.IntakeId && x.TenantId == tenantId, ct);

        if (intake == null)
            throw new KeyNotFoundException("Intake not found");

        if (!intake.PatientId.HasValue)
            throw new InvalidOperationException("Intake has no associated patient");

        // 2. Get Patient
        var patient = await _context.Patients
            .FirstOrDefaultAsync(p => p.Id == intake.PatientId.Value && p.TenantId == tenantId, ct);

        if (patient == null)
            throw new KeyNotFoundException("Patient not found");

        // 3. Get Visit
        var visit = await _context.Visits
            .FirstOrDefaultAsync(v => v.PatientId == patient.Id && v.TenantId == tenantId, ct);

        // 4. Get Room Number
        var roomNumber = "-";
        if (visit != null)
        {
            roomNumber = await _context.RoomAssignments
                .Where(ra => ra.VisitId == visit.Id && ra.IsActive && ra.TenantId == tenantId)
                .Select(ra => ra.Room.RoomNumber)
                .FirstOrDefaultAsync(ct) ?? "-";
        }

        // 5. Generate QR (MedicalNumber | VisitId)
        var qrBytes = _qr.Generate($"{patient.MedicalNumber}|{visit?.Id ?? Guid.Empty}");

        return new WristbandDto
        {
            PatientName = patient.FullName,
            MedicalNumber = patient.MedicalNumber,
            RoomNumber = roomNumber,
            QrCode = qrBytes
        };
    }
}
