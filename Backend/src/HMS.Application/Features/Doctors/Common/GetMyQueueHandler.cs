using HMS.Application.Abstractions.Persistence;
using HMS.Application.Abstractions.CurrentUser;
using HMS.Application.Features.Doctors.Common;
using MediatR;
using HMS.Domain.Enums;
using Microsoft.EntityFrameworkCore;

public class GetMyQueueHandler : IRequestHandler<GetMyQueueQuery, List<DoctorQueueDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public GetMyQueueHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<DoctorQueueDto>> Handle(GetMyQueueQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        var tenantId = _currentUser.TenantId;

        if (userId == Guid.Empty)
            throw new UnauthorizedAccessException("Invalid user");

        var queue = await _context.Visits
            .AsNoTracking() // ⚡ performance
            .Where(x =>
                x.TenantId == tenantId && // 💣 مهم جدًا
                x.DoctorId == userId &&
                x.Status != VisitStatus.Completed &&
                x.VisitType != VisitType.Inpatient // 👈 optional حسب السيستم
            )
            .OrderBy(x => x.QueueNumber)
            .Select(x => new DoctorQueueDto
            {
                VisitId = x.Id,
                PatientName = x.Patient != null ? x.Patient.FullName : "Unknown",
                QueueNumber = x.QueueNumber,
                Status = x.Status.ToString()
            })
            .ToListAsync(cancellationToken);

        return queue;
    }
}