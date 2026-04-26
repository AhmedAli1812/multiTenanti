using HMS.Application.Abstractions.Persistence;
using HMS.Application.Abstractions.Tenant;
using HMS.Domain.Entities.Doctors;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HMS.Application.Features.Doctors.CreateProfile;

public class CreateDoctorProfileHandler : IRequestHandler<CreateDoctorProfileCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantProvider _tenant;

    public CreateDoctorProfileHandler(IApplicationDbContext context, ITenantProvider tenant)
    {
        _context = context;
        _tenant = tenant;
    }

    public async Task<Guid> Handle(CreateDoctorProfileCommand request, CancellationToken cancellationToken)
    {
        // =========================
        // 💣 Validation
        // =========================
        if (request.UserId == Guid.Empty)
            throw new ArgumentException("Invalid user");

        if (string.IsNullOrWhiteSpace(request.Specialty))
            throw new ArgumentException("Specialty is required");

        if (request.YearsOfExperience < 0 || request.YearsOfExperience > 60)
            throw new ArgumentException("Invalid years of experience");

        var tenantId = _tenant.GetTenantId();
        var specialty = request.Specialty.Trim();

        // =========================
        // 🔥 Check User exists
        // =========================
        var userExists = await _context.Users
            .AsNoTracking()
            .AnyAsync(u => u.Id == request.UserId && u.TenantId == tenantId, cancellationToken);

        if (!userExists)
            throw new InvalidOperationException("User not found");

        // =========================
        // 🔥 Prevent duplicate profile
        // =========================
        var exists = await _context.DoctorProfiles
            .AsNoTracking()
            .AnyAsync(x => x.UserId == request.UserId && x.TenantId == tenantId, cancellationToken);

        if (exists)
            throw new InvalidOperationException("Doctor profile already exists");

        // =========================
        // 🔥 Create Profile
        // =========================
        var profile = new DoctorProfile
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            Specialty = specialty,
            YearsOfExperience = request.YearsOfExperience,
            TenantId = tenantId
        };

        await _context.DoctorProfiles.AddAsync(profile, cancellationToken);

        // =========================
        // 💾 Save
        // =========================
        await _context.SaveChangesAsync(cancellationToken);

        return profile.Id;
    }
}