using HMS.Application.Abstractions.Caching;
using HMS.Application.Abstractions.CurrentUser;
using HMS.Application.Abstractions.Persistence;
using HMS.Application.Dtos;
using HMS.Application.Features.Permissions;
using MediatR;
using Microsoft.EntityFrameworkCore;

public class GetAllPermissionsHandler
    : IRequestHandler<GetAllPermissionsQuery, List<PermissionDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IPermissionCacheService _cache;
    private readonly ICurrentUser _currentUser;

    public GetAllPermissionsHandler(
        IApplicationDbContext context,
        IPermissionCacheService cache,
        ICurrentUser currentUser)
    {
        _context = context;
        _cache = cache;
        _currentUser = currentUser;
    }

    public async Task<List<PermissionDto>> Handle(
        GetAllPermissionsQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;

        // Removed cache check here because it was returning PermissionDto without IDs, 
        // causing selection issues in the frontend. We still update the cache below.

        // =========================
        // 💾 DB
        // =========================
        var permissions = await _context.Permissions
            .AsNoTracking()
            .OrderBy(p => p.Module)
            .ThenBy(p => p.Action)
            .Select(p => new PermissionDto
            {
                Id = p.Id,
                Code = p.Code,
                Module = p.Module,
                Action = p.Action
            })
            .ToListAsync(cancellationToken);

        // =========================
        // 🧠 Cache
        // =========================
        await _cache.SetAllPermissionsAsync(
            userId,
            permissions.Select(p => p.Code).ToList()
        );

        return permissions;
    }
}