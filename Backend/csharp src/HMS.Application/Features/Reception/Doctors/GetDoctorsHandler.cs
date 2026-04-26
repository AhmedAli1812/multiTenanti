using HMS.Application.Abstractions.Persistence;
using HMS.Application.Abstractions.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HMS.Application.Features.Reception.Doctors;

public class GetDoctorsHandler : IRequestHandler<GetDoctorsQuery, List<DoctorLookupDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public GetDoctorsHandler(
        IApplicationDbContext context,
        ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<DoctorLookupDto>> Handle(
        GetDoctorsQuery request,
        CancellationToken cancellationToken)
    {
        // =========================
        // 🔥 Base Query
        // =========================
        var query = _context.Users.AsNoTracking();

        // =========================
        // 🔥 Super Admin يشوف كله
        // =========================
        if (_currentUser.IsGlobal)
        {
            query = query.IgnoreQueryFilters();
        }

        // =========================
        // 💣 Get Doctor Role (DYNAMIC)
        // =========================
        var doctorRole = await _context.Roles
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(r => 
                r.Name.ToLower() == "doctor" ||
                r.NormalizedName == "DOCTOR", // إذا كانت عندك normalized name
            cancellationToken);

        if (doctorRole == null)
            return new List<DoctorLookupDto>();

        var doctorRoleId = doctorRole.Id;

        // =========================
        // 💣 Filter Doctors
        // =========================
        query = query.Where(u =>
            u.UserRoles.Any(ur => ur.RoleId == doctorRoleId));

        // =========================
        // 🔥 Branch Logic
        // =========================

        if (request.BranchId.HasValue)
        {
            query = query.Where(u => u.BranchId == request.BranchId);
        }
        else if (!_currentUser.IsGlobal && _currentUser.BranchId.HasValue)
        {
            query = query.Where(u => u.BranchId == _currentUser.BranchId);
        }
        else if (!_currentUser.IsGlobal && !_currentUser.BranchId.HasValue)
        {
            return new List<DoctorLookupDto>();
        }

        // =========================
        // 🔥 Department Filter
        // =========================
        if (request.DepartmentId.HasValue)
        {
            query = query.Where(u => u.DepartmentId == request.DepartmentId);
        }

        // =========================
        // 📄 Result
        // =========================
        var result = await query
            .OrderBy(u => u.FullName)
            .Select(u => new DoctorLookupDto
            {
                Id = u.Id,
                Name = u.FullName
            })
            .ToListAsync(cancellationToken);

        // أضف هذا في الـ handler للعثور على الاسم الفعلي
        var allRoles = await _context.Roles
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Select(r => r.Name)
            .ToListAsync(cancellationToken);

        // Debug: شوف إيه الأدوار الموجودة فعلاً
        System.Diagnostics.Debug.WriteLine($"Available Roles: {string.Join(", ", allRoles)}

        return result;
    }
}