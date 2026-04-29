using MediatR;

namespace HMS.Application.Features.Users.CreateUser;

public class CreateUserCommand : IRequest<Guid>
{
    public string FullName { get; set; }
    public string Password { get; set; }

    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Username { get; set; }      // ✅ أضف دي
    public string? NationalId { get; set; }    // ✅ أضف دي
    public Guid? DepartmentId { get; set; }
    public Guid? BranchId { get; set; }
    public Guid? TenantId { get; set; }
    public List<Guid> RoleIds { get; set; } = new();
}