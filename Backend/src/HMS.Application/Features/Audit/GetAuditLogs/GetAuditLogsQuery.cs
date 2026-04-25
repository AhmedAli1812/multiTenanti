using MediatR;

public class GetAuditLogsQuery : IRequest<PaginatedAuditLogsDto>
{
    public string? Action { get; set; }
    public Guid? UserId { get; set; }

    public DateTime? From { get; set; }
    public DateTime? To { get; set; }

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}