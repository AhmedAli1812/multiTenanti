namespace HMS.Application.Features.Auth.Login;

public class LoginResponse
{
    public string AccessToken { get; set; } = default!;
    public string RefreshToken { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public string DeviceId { get; set; } = default!;
}