namespace HMS.Application.Abstractions.Services
{
    public interface ICurrentUserService
    {
        Guid UserId { get; }
        Guid TenantId { get; }

    }
}