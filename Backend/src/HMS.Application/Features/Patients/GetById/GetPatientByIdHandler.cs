using HMS.Application.Abstractions.Persistence;
using HMS.Application.Abstractions.CurrentUser;
using HMS.Application.Features.Patients.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HMS.Application.Features.Patients.GetById;

public class GetPatientByIdHandler : IRequestHandler<GetPatientByIdQuery, PatientDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public GetPatientByIdHandler(
        IApplicationDbContext context,
        ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<PatientDto> Handle(
        GetPatientByIdQuery request,
        CancellationToken cancellationToken)
    {
        var tenantId = _currentUser.TenantId;

        var patient = await _context.Patients
            .AsNoTracking() // ⚡ performance
            .Where(x =>
                x.Id == request.Id &&
                x.TenantId == tenantId &&   // 💣 SaaS isolation
                !x.IsDeleted)               // 💣 soft delete
            .Select(x => new PatientDto
            {
                Id = x.Id,
                FullName = x.FullName,
                MedicalNumber = x.MedicalNumber,
                PhoneNumber = x.PhoneNumber
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (patient == null)
            throw new InvalidOperationException("Patient not found");

        return patient;
    }
}