using MediatR;

namespace HMS.Application.Features.Auth.Logout;

public class LogoutCommand : IRequest<Unit>
{
    public Guid UserId { get; set; }
}