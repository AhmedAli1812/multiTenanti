using HMS.Application.Features.Auth.Login;
using HMS.Shared.Results;
using MediatR;

namespace HMS.Application.Features.Auth.Refresh;

public class RefreshTokenCommand : IRequest<Result<LoginResponse>>
{
    public string RefreshToken { get; set; } = default!;
    public string DeviceId { get; set; } = default!;
}