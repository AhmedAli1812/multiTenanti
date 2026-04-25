using HMS.Application.Abstractions.Persistence;
using HMS.Application.Abstractions.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

public class GetUnreadCountHandler : IRequestHandler<GetUnreadCountQuery, int>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetUnreadCountHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<int> Handle(GetUnreadCountQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        var tenantId = _currentUser.TenantId;

        // =========================
        // 💣 Validation
        // =========================
        if (userId == Guid.Empty)
            throw new UnauthorizedAccessException("Invalid user");

        // =========================
        // 🔥 Count
        // =========================
        var count = await _context.Notifications
            .AsNoTracking() // ⚡ performance
            .Where(x =>
                x.UserId == userId &&
                x.TenantId == tenantId && // 💣 مهم جدًا
                !x.IsRead)
            .CountAsync(cancellationToken);

        return count;
    }
}