using HMS.Application.Abstractions.Persistence;
using HMS.Application.Abstractions.Services;
using HMS.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HMS.Application.Features.Notifications.Queries.GetMyNotifications
{
    public class GetMyNotificationsHandler
        : IRequestHandler<GetMyNotificationsQuery, List<NotificationDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;

        public GetMyNotificationsHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<List<NotificationDto>> Handle(
            GetMyNotificationsQuery request,
            CancellationToken cancellationToken)
        {
            var userId = _currentUser.UserId;
            var tenantId = _currentUser.TenantId; // 💣 مهم جدًا

            if (userId == Guid.Empty)
                throw new UnauthorizedAccessException("Invalid user");

            var page = request.Page <= 0 ? 1 : request.Page;
            var pageSize = request.PageSize <= 0 ? 10 : request.PageSize;

            return await _context.Notifications
                .AsNoTracking()
                .Where(x =>
                    x.UserId == userId &&
                    x.TenantId == tenantId // 💣 دي غالبًا سبب المشكلة لو مش موجودة
                )
                .OrderByDescending(x => x.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new NotificationDto
                {
                    Id = x.Id,
                    Title = x.Title,
                    Message = x.Message,
                    Type = x.Type,
                    IsRead = x.IsRead,
                    CreatedAt = x.CreatedAt
                })
                .ToListAsync(cancellationToken);
        }
    }
}