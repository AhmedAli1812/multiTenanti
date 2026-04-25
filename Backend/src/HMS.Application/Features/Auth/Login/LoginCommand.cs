using MediatR;

namespace HMS.Application.Features.Auth.Login;

public class LoginCommand : IRequest<LoginResponse>
{
    public string Identifier { get; set; } = default!;
    public string Password { get; set; } = default!;
}