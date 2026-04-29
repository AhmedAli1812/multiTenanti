
namespace HMS.Application.Features.Users.GetUsers;
public class UserDto
{
    public Guid Id { get; set; } // 🔥 مهم

    public string FullName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Username { get; set; }
    public string? NationalId { get; set; }
    public DateTime CreatedAt { get; set; }

    public List<string> Roles { get; set; } = new();
}