using MediatR;
using HMS.Shared.Results;

namespace HMS.Application.Features.Users.DeleteUser
{
    public class DeleteUserCommand : IRequest<Result>
    {
        public Guid Id { get; set; }

        public DeleteUserCommand(Guid id)
        {
            Id = id;
        }
    }
}