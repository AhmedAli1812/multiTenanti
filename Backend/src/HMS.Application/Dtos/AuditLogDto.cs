namespace HMS.Application.Dtos
{
    public class AuditLogDto
    {
        public Guid Id { get; set; }
        public string? UserName { get; set; }
        public string Action { get; set; } = default!;
        public string EntityName { get; set; } = default!;
        public string? EntityId { get; set; }
        public string? IPAddress { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}