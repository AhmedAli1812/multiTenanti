using HMS.Application.Abstractions.Persistence;
using HMS.Application.Abstractions.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HMS.Application.Features.Users.GetUsers;

public class GetUsersHandler : IRequestHandler<GetUsersQuery, PaginatedResult<UserDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public GetUsersHandler(
        IApplicationDbContext context,
        ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<PaginatedResult<UserDto>> Handle(
        GetUsersQuery request,
        CancellationToken cancellationToken)
    {
        var tenantId = _currentUser.TenantId;

        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 10 : request.PageSize;
        if (pageSize > 50) pageSize = 50;

        var search = request.Search?.Trim();

        // =========================
        // 🧠 Base Query
        // =========================
        var usersQuery = _context.Users
            .AsNoTracking()
            .Where(u => !u.IsDeleted);

        // 🔥 Multi-Tenant Logic
        if (!_currentUser.IsGlobal)
        {
            usersQuery = usersQuery.Where(u => u.TenantId == tenantId);
        }

        // =========================
        // 🔍 Search
        // =========================
        if (!string.IsNullOrWhiteSpace(search))
        {
            usersQuery = usersQuery.Where(u =>
                u.FullName.Contains(search) ||
                (u.Email != null && u.Email.Contains(search)) ||
                (u.PhoneNumber != null && u.PhoneNumber.Contains(search)));
        }

        // =========================
        // 📊 Count
        // =========================
        var totalCount = await usersQuery.CountAsync(cancellationToken);

        // =========================
        // 📄 Load Users (Paged)
        // =========================
        var users = await usersQuery
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new
            {
                u.Id,
                u.FullName,
                u.Email,
                u.PhoneNumber,
                u.Username,
                u.NationalId
            })
            .ToListAsync(cancellationToken);

        var userIds = users.Select(u => u.Id).ToList();

        // =========================
        // 🔥 Load Roles (Optimized)
        // =========================
        var rolesQuery = _context.UserRoles
            .AsNoTracking()
            .Where(ur => userIds.Contains(ur.UserId));

        // 🔥 برضو هنا Multi-Tenant
        if (!_currentUser.IsGlobal)
        {
            rolesQuery = rolesQuery.Where(ur => ur.TenantId == tenantId);
        }

        var roles = await rolesQuery
            .Join(_context.Roles,
                ur => ur.RoleId,
                r => r.Id,
                (ur, r) => new
                {
                    ur.UserId,
                    RoleName = r.Name
                })
            .ToListAsync(cancellationToken);

        // =========================
        // 🧠 Map
        // =========================
        var items = users.Select(u => new UserDto
        {
            Id = u.Id,
            FullName = u.FullName,
            Email = u.Email,
            PhoneNumber = u.PhoneNumber,
            Username = u.Username,
            NationalId = u.NationalId,

            Roles = roles
                .Where(r => r.UserId == u.Id)
                .Select(r => r.RoleName)
                .Distinct()
                .ToList()
        }).ToList();

        // =========================
        // 🎯 Filter by Role (optional)
        // =========================
        if (!string.IsNullOrWhiteSpace(request.Role))
        {
            items = items
                .Where(u => u.Roles.Contains(request.Role))
                .ToList();
        }

        return new PaginatedResult<UserDto>
        {
            Items = items,
            PageNumber = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }
}