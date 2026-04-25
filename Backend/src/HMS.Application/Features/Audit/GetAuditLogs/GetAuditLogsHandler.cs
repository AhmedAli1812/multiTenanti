using HMS.Application.Abstractions.Persistence;
using HMS.Application.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

public class GetAuditLogsHandler
    : IRequestHandler<GetAuditLogsQuery, PaginatedAuditLogsDto>
{
    private readonly IApplicationDbContext _context;

    public GetAuditLogsHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedAuditLogsDto> Handle(
        GetAuditLogsQuery request,
        CancellationToken cancellationToken)
    {
        // 💣 Validation
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 10 : request.PageSize;

        if (pageSize > 100)
            pageSize = 100; // 🔥 Limit علشان الأداء

        var query = _context.AuditLogs
            .AsNoTracking()
            .AsQueryable();

        // =========================
        // 🔍 Filters
        // =========================

        if (!string.IsNullOrWhiteSpace(request.Action))
        {
            var action = request.Action.Trim();
            query = query.Where(x => x.Action == action);
        }

        if (request.UserId.HasValue)
        {
            query = query.Where(x => x.UserId == request.UserId.Value);
        }

        if (request.From.HasValue)
        {
            query = query.Where(x => x.CreatedAt >= request.From.Value);
        }

        if (request.To.HasValue)
        {
            query = query.Where(x => x.CreatedAt <= request.To.Value);
        }

        // =========================
        // 🔢 Total Count
        // =========================
        var totalCount = await query.CountAsync(cancellationToken);

        // =========================
        // 📄 Data
        // =========================
        var data = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new AuditLogDto
            {
                Id = x.Id,
                UserName = x.UserName,
                Action = x.Action,
                EntityName = x.EntityName,
                EntityId = x.EntityId,
                IPAddress = x.IPAddress,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return new PaginatedAuditLogsDto
        {
            TotalCount = totalCount,
            Data = data
        };
    }
}