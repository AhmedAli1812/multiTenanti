
using HMS.Application.Dtos;
public class PaginatedAuditLogsDto
{
    public int TotalCount { get; set; }
    public List<AuditLogDto> Data { get; set; } = new();
}