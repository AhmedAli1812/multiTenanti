namespace HMS.Application.Abstractions.CurrentUser;

public interface ICurrentUser
{
    Guid UserId { get; }
    Guid TenantId { get; }
    string Role { get; }
    bool IsGlobal { get; }
    Guid? BranchId { get; }
}